# Development Environment Outputs
# These outputs pass through values from the root infrastructure module

# ============================================================================
# ACM Certificate Outputs
# ============================================================================

output "acm_certificate_arn" {
  description = "ARN of the ACM certificate"
  value       = module.infrastructure.acm_certificate_arn
}

output "acm_certificate_status" {
  description = "Status of the ACM certificate (PENDING_VALIDATION, ISSUED, etc.)"
  value       = module.infrastructure.acm_certificate_status
}

output "acm_validation_records" {
  description = "🔐 DNS validation records - ADD THESE TO OVH DNS"
  value       = module.infrastructure.acm_validation_records
}

# ============================================================================
# Frontend Outputs
# ============================================================================

# Frontend outputs
output "frontend_bucket_name" {
  description = "Name of the S3 bucket hosting the frontend"
  value       = module.infrastructure.frontend_bucket_name
}

output "cloudfront_distribution_id" {
  description = "CloudFront distribution ID"
  value       = module.infrastructure.cloudfront_distribution_id
}

output "cloudfront_domain_name" {
  description = "CloudFront distribution domain name"
  value       = module.infrastructure.cloudfront_domain_name
}

output "frontend_url" {
  description = "Frontend URL (use this for DNS configuration)"
  value       = module.infrastructure.frontend_url
}

# Backend outputs
output "ecr_repository_url" {
  description = "ECR repository URL for backend Docker images"
  value       = module.infrastructure.ecr_repository_url
}

output "alb_dns_name" {
  description = "Application Load Balancer DNS name"
  value       = module.infrastructure.alb_dns_name
}

output "alb_zone_id" {
  description = "Application Load Balancer hosted zone ID"
  value       = module.infrastructure.alb_zone_id
}

output "backend_url" {
  description = "Backend API URL (use this for DNS configuration)"
  value       = module.infrastructure.backend_url
}

output "ecs_cluster_name" {
  description = "ECS cluster name"
  value       = module.infrastructure.ecs_cluster_name
}

output "ecs_service_name" {
  description = "ECS service name"
  value       = module.infrastructure.ecs_service_name
}

# Networking outputs
output "vpc_id" {
  description = "VPC ID"
  value       = module.infrastructure.vpc_id
}

output "public_subnet_ids" {
  description = "Public subnet IDs"
  value       = module.infrastructure.public_subnet_ids
}

# Parameter Store outputs
output "parameter_store_paths" {
  description = "Parameter Store parameter paths"
  value       = module.infrastructure.parameter_store_paths
}

output "parameter_store_setup_commands" {
  description = "AWS CLI commands to set Parameter Store values"
  value       = module.infrastructure.parameter_store_setup_commands
}

# DNS Configuration Instructions
output "dns_configuration_ovh" {
  description = "Instructions for DNS configuration in OVH"
  value       = module.infrastructure.dns_configuration_ovh
}

# GitHub Actions Secrets
output "github_actions_secrets" {
  description = "GitHub Actions secrets to configure"
  value       = module.infrastructure.github_actions_secrets
  sensitive   = true
}

