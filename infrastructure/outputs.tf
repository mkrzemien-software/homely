# Terraform Outputs
# These values are displayed after successful deployment

# ACM outputs
output "acm_certificate_arn" {
  description = "ARN of the ACM certificate"
  value       = module.acm.certificate_arn
}

output "acm_certificate_status" {
  description = "Status of the ACM certificate (PENDING_VALIDATION, ISSUED, etc.)"
  value       = module.acm.certificate_status
}

output "acm_validation_records" {
  description = "DNS validation records to add to OVH DNS"
  value       = module.acm.validation_records
}

# Frontend outputs
output "frontend_bucket_name" {
  description = "Name of the S3 bucket hosting the frontend"
  value       = module.frontend.bucket_name
}

output "cloudfront_distribution_id" {
  description = "CloudFront distribution ID"
  value       = module.frontend.cloudfront_distribution_id
}

output "cloudfront_domain_name" {
  description = "CloudFront distribution domain name"
  value       = module.frontend.cloudfront_domain_name
}

output "frontend_url" {
  description = "Frontend URL (use this for DNS configuration)"
  value       = "https://${var.frontend_subdomain}.${var.domain_name}"
}

# Backend outputs
output "ecr_repository_url" {
  description = "ECR repository URL for backend Docker images"
  value       = module.backend.ecr_repository_url
}

output "alb_dns_name" {
  description = "Application Load Balancer DNS name"
  value       = module.backend.alb_dns_name
}

output "alb_zone_id" {
  description = "Application Load Balancer hosted zone ID"
  value       = module.backend.alb_zone_id
}

output "backend_url" {
  description = "Backend API URL (use this for DNS configuration)"
  value       = "https://${var.backend_subdomain}.${var.domain_name}"
}

output "ecs_cluster_name" {
  description = "ECS cluster name"
  value       = module.backend.ecs_cluster_name
}

output "ecs_service_name" {
  description = "ECS service name"
  value       = module.backend.ecs_service_name
}

# Networking outputs
output "vpc_id" {
  description = "VPC ID"
  value       = module.networking.vpc_id
}

output "public_subnet_ids" {
  description = "Public subnet IDs"
  value       = module.networking.public_subnet_ids
}

# Parameter Store outputs
output "parameter_store_paths" {
  description = "Parameter Store parameter paths"
  value = {
    supabase_url      = module.parameters.parameter_paths.supabase_url
    supabase_anon_key = module.parameters.parameter_paths.supabase_anon_key
    backend_log_level = module.parameters.parameter_paths.backend_log_level
    cors_origins      = module.parameters.parameter_paths.cors_origins
  }
}

output "parameter_store_setup_commands" {
  description = "AWS CLI commands to set Parameter Store values"
  value = <<-EOT
    # Set Supabase URL
    aws ssm put-parameter \
      --name "${module.parameters.parameter_paths.supabase_url}" \
      --value "YOUR_SUPABASE_URL" \
      --type "SecureString" \
      --region ${var.aws_region} \
      --overwrite

    # Set Supabase Anon Key
    aws ssm put-parameter \
      --name "${module.parameters.parameter_paths.supabase_anon_key}" \
      --value "YOUR_SUPABASE_ANON_KEY" \
      --type "SecureString" \
      --region ${var.aws_region} \
      --overwrite

    # Set Backend Log Level
    aws ssm put-parameter \
      --name "${module.parameters.parameter_paths.backend_log_level}" \
      --value "Information" \
      --type "String" \
      --region ${var.aws_region} \
      --overwrite

    # Set CORS Origins
    aws ssm put-parameter \
      --name "${module.parameters.parameter_paths.cors_origins}" \
      --value "https://${var.frontend_subdomain}.${var.domain_name}" \
      --type "String" \
      --region ${var.aws_region} \
      --overwrite
  EOT
}

# DNS Configuration Instructions
output "dns_configuration_ovh" {
  description = "Instructions for DNS configuration in OVH"
  value = <<-EOT
    ========================================
    OVH DNS Configuration Instructions
    ========================================

    Add the following DNS records in your OVH domain control panel:

    1. Frontend (CloudFront):
       Type: CNAME
       Name: ${var.frontend_subdomain}
       Target: ${module.frontend.cloudfront_domain_name}
       TTL: 3600

    2. Backend (ALB):
       Type: CNAME
       Name: ${var.backend_subdomain}
       Target: ${module.backend.alb_dns_name}
       TTL: 3600

    3. SSL Certificate Validation:
       After adding the above records, you need to validate the SSL certificates.
       Check AWS Certificate Manager console for DNS validation records.
       Add those CNAME records to OVH DNS.

    NOTE: DNS propagation can take up to 48 hours, but typically completes within 1-2 hours.
  EOT
}

# GitHub Actions Secrets
output "github_actions_secrets" {
  description = "GitHub Actions secrets to configure"
  value = <<-EOT
    ========================================
    GitHub Actions Secrets Configuration
    ========================================

    Add these secrets to your GitHub repository:
    Settings → Secrets and variables → Actions → New repository secret

    AWS_ACCOUNT_ID=${data.aws_caller_identity.current.account_id}
    AWS_REGION=${var.aws_region}
    ECR_REPOSITORY_URL=${module.backend.ecr_repository_url}
    ECS_CLUSTER_NAME=${module.backend.ecs_cluster_name}
    ECS_SERVICE_NAME=${module.backend.ecs_service_name}
    FRONTEND_S3_BUCKET=${module.frontend.bucket_name}
    CLOUDFRONT_DISTRIBUTION_ID=${module.frontend.cloudfront_distribution_id}

    For AWS authentication, configure OIDC (recommended) or use AWS access keys.
    See README.md for detailed instructions.
  EOT
  sensitive = true
}

# Data source for current AWS account
data "aws_caller_identity" "current" {}
