# Production Environment Configuration
#
# This file uses the root Terraform modules to create prod environment infrastructure
# Run from infrastructure root directory: terraform -chdir=environments/prod init/plan/apply
#
# Variables are defined in variables.tf and can be customized in terraform.tfvars

# Use root module
module "infrastructure" {
  source = "../.."

  # Project Configuration
  project_name = var.project_name
  environment  = var.environment
  aws_region   = var.aws_region

  # Domain Configuration
  domain_name        = var.domain_name
  frontend_subdomain = var.frontend_subdomain
  backend_subdomain  = var.backend_subdomain

  # Backend Configuration
  docker_image_tag          = var.docker_image_tag
  backend_cpu               = var.backend_cpu
  backend_memory            = var.backend_memory
  backend_port              = var.backend_port
  backend_health_check_path = var.backend_health_check_path

  # GitHub Configuration
  github_repo = var.github_repo

  # Cost Optimization Settings (balanced for prod)
  use_fargate_spot              = var.use_fargate_spot
  use_single_az                 = var.use_single_az
  cloudwatch_log_retention_days = var.cloudwatch_log_retention_days
  enable_alb_access_logs        = var.enable_alb_access_logs

  # Tagging
  cost_center = var.cost_center

  additional_tags = var.additional_tags
}
