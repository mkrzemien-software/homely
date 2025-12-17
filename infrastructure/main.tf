# Main Terraform Configuration
# Orchestrates all infrastructure modules

locals {
  name_prefix = "${var.project_name}-${var.environment}"

  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
    CostCenter  = var.cost_center
  }

  # Computed domain names
  frontend_domain = "${var.frontend_subdomain}.${var.domain_name}"
  backend_domain  = "${var.backend_subdomain}.${var.domain_name}"
}

# Networking Module
# Creates VPC, subnets, internet gateway, and routing
module "networking" {
  source = "./modules/networking"

  project_name = var.project_name
  environment  = var.environment
  aws_region   = var.aws_region

  vpc_cidr           = var.use_single_az ? "10.0.0.0/24" : "10.0.0.0/16"
  use_single_az      = var.use_single_az
  availability_zones = var.use_single_az ? 1 : 2

  tags = local.common_tags
}

# Security Module
# Creates security groups and IAM roles
module "security" {
  source = "./modules/security"

  project_name = var.project_name
  environment  = var.environment

  vpc_id               = module.networking.vpc_id
  github_repo          = var.github_repo
  github_oidc_thumbprint = var.github_oidc_thumbprint

  tags = local.common_tags
}

# Parameters Module
# Creates AWS Systems Manager Parameter Store parameters
module "parameters" {
  source = "./modules/parameters"

  project_name = var.project_name
  environment  = var.environment

  # Default values (will be overwritten manually or by CI/CD)
  supabase_url                = var.supabase_url
  supabase_anon_key           = var.supabase_anon_key
  supabase_service_role_key   = var.supabase_service_role_key
  database_connection_string  = var.database_connection_string

  # JWT configuration
  jwt_valid_issuer   = var.jwt_valid_issuer
  jwt_valid_audience = var.jwt_valid_audience
  jwt_secret         = var.jwt_secret

  # Non-sensitive defaults
  backend_log_level = var.environment == "prod" ? "Warning" : "Information"
  cors_origins      = "https://${local.frontend_domain}"

  tags = local.common_tags
}

# Backend Module
# Creates ECR, ECS Fargate, ALB, and related resources
module "backend" {
  source = "./modules/backend"

  project_name = var.project_name
  environment  = var.environment
  aws_region   = var.aws_region

  # Networking
  vpc_id            = module.networking.vpc_id
  public_subnet_ids = module.networking.public_subnet_ids

  # ECS Configuration
  docker_image_tag  = var.docker_image_tag
  task_cpu          = var.backend_cpu
  task_memory       = var.backend_memory
  container_port    = 8080
  desired_count     = 1
  use_fargate_spot  = var.use_fargate_spot

  # Health Check
  health_check_path = var.backend_health_check_path

  # Security
  alb_security_group_id      = module.security.alb_security_group_id
  ecs_security_group_id      = module.security.ecs_security_group_id
  ecs_task_execution_role_arn = module.security.ecs_task_execution_role_arn
  ecs_task_role_arn          = module.security.ecs_task_role_arn

  # Parameter Store paths for environment variables
  parameter_store_paths = module.parameters.parameter_paths

  # ACM Certificate ARN from ACM module
  # Uses the same wildcard certificate as CloudFront (*.maflint.com)
  acm_certificate_arn = module.acm.certificate_arn

  # Enable HTTPS (set to true to create HTTPS listener)
  # Using explicit boolean to avoid count dependency issues during destroy
  enable_https = true

  # Monitoring
  cloudwatch_log_retention_days = var.cloudwatch_log_retention_days
  enable_alb_access_logs        = var.enable_alb_access_logs
  alb_access_logs_bucket        = var.enable_alb_access_logs ? module.monitoring[0].alb_logs_bucket_name : ""

  tags = local.common_tags
}

# ACM Module (for CloudFront SSL certificate)
# Creates SSL certificate in us-east-1 for CloudFront custom domain
module "acm" {
  source = "./modules/acm"

  providers = {
    aws.us_east_1 = aws.us_east_1
  }

  project_name = var.project_name
  environment  = var.environment

  # Request wildcard certificate for all subdomains
  domain_name = "*.${var.domain_name}"
  
  # Also include root domain
  subject_alternative_names = [var.domain_name]

  tags = local.common_tags
}

# Frontend Module
# Creates S3 bucket and CloudFront distribution
module "frontend" {
  source = "./modules/frontend"

  project_name = var.project_name
  environment  = var.environment

  domain_name = local.frontend_domain

  # Pass certificate ARN from ACM module
  # NOTE: CloudFront will fail if certificate is not ISSUED
  # This is expected - workflow will wait for validation then re-apply
  acm_certificate_arn = module.acm.certificate_arn

  # CORS configuration for API calls
  cors_allowed_origins = ["https://${local.backend_domain}"]

  tags = local.common_tags
}

# Monitoring Module (optional, only if ALB access logs enabled)
module "monitoring" {
  count  = var.enable_alb_access_logs ? 1 : 0
  source = "./modules/monitoring"

  project_name = var.project_name
  environment  = var.environment
  aws_region   = var.aws_region

  # CloudWatch configuration
  log_retention_days = var.cloudwatch_log_retention_days

  # ECS monitoring
  ecs_cluster_name = module.backend.ecs_cluster_name
  ecs_service_name = module.backend.ecs_service_name

  # ALB monitoring
  alb_arn = module.backend.alb_arn

  tags = local.common_tags
}
