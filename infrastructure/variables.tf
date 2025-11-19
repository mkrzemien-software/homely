# Root variables for Terraform AWS infrastructure
# These variables are used across all modules

variable "project_name" {
  description = "Name of the project (used for resource naming and tagging)"
  type        = string
  default     = "homely"

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.project_name))
    error_message = "Project name must contain only lowercase letters, numbers, and hyphens."
  }
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "aws_region" {
  description = "AWS region for all resources"
  type        = string
  default     = "us-east-1"
}

# Domain configuration
variable "domain_name" {
  description = "Root domain name (e.g., example.com) - Configure in OVH DNS"
  type        = string
  default     = "yourdomain.com"
}

variable "frontend_subdomain" {
  description = "Subdomain for frontend application (e.g., app)"
  type        = string
  default     = "app"
}

variable "backend_subdomain" {
  description = "Subdomain for backend API (e.g., api)"
  type        = string
  default     = "api"
}

# Backend configuration
variable "docker_image_tag" {
  description = "Docker image tag for backend deployment"
  type        = string
  default     = "latest"
}

variable "backend_cpu" {
  description = "CPU units for ECS task (256 = 0.25 vCPU)"
  type        = number
  default     = 256
}

variable "backend_memory" {
  description = "Memory for ECS task in MB"
  type        = number
  default     = 512
}

variable "backend_port" {
  description = "Port on which backend application listens"
  type        = number
  default     = 5000
}

variable "backend_health_check_path" {
  description = "Health check endpoint path"
  type        = string
  default     = "/health"
}

# Supabase configuration (stored in Parameter Store)
variable "supabase_url" {
  description = "Supabase project URL"
  type        = string
  sensitive   = true
  default     = "" # PLACEHOLDER: Will be stored in Parameter Store
}

variable "supabase_anon_key" {
  description = "Supabase anonymous key"
  type        = string
  sensitive   = true
  default     = "" # PLACEHOLDER: Will be stored in Parameter Store
}

# GitHub configuration
variable "github_repo" {
  description = "GitHub repository name (owner/repo)"
  type        = string
  default     = "yourusername/homely" # PLACEHOLDER: Replace with your GitHub repo
}

variable "github_oidc_thumbprint" {
  description = "Thumbprint for GitHub OIDC provider"
  type        = string
  default     = "6938fd4d98bab03faadb97b34396831e3780aea1"
}

# Cost optimization settings
variable "use_fargate_spot" {
  description = "Use Fargate Spot for cost savings (up to 70% cheaper)"
  type        = bool
  default     = true
}

variable "use_single_az" {
  description = "Use single availability zone instead of multi-AZ (lower cost, lower availability)"
  type        = bool
  default     = true
}

variable "cloudwatch_log_retention_days" {
  description = "Number of days to retain CloudWatch logs"
  type        = number
  default     = 7
}

variable "enable_alb_access_logs" {
  description = "Enable ALB access logs (additional S3 costs)"
  type        = bool
  default     = false
}

# Tagging
variable "cost_center" {
  description = "Cost center for resource tagging"
  type        = string
  default     = "Engineering"
}

variable "additional_tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default     = {}
}
