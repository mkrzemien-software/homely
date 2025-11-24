# Development Environment Variables
#
# These variables can be set in terraform.tfvars file
# Most have sensible defaults for dev environment

variable "domain_name" {
  description = "Root domain name (e.g., homely.maflint.com)"
  type        = string
  default     = "homely.maflint.com"
}

variable "frontend_subdomain" {
  description = "Subdomain for frontend application"
  type        = string
  default     = "dev"
}

variable "backend_subdomain" {
  description = "Subdomain for backend API"
  type        = string
  default     = "dev-api"
}

variable "github_repo" {
  description = "GitHub repository name (owner/repo)"
  type        = string
  default     = "mkrzemien/homely"
}

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
  default     = 8080
}

variable "backend_health_check_path" {
  description = "Health check endpoint path"
  type        = string
  default     = "/health"
}

variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "homely"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "AWS region for all resources"
  type        = string
  default     = "us-east-1"
}

variable "use_fargate_spot" {
  description = "Use Fargate Spot for cost savings"
  type        = bool
  default     = true
}

variable "use_single_az" {
  description = "Use single availability zone (NOTE: ALB requires 2 AZs minimum)"
  type        = bool
  default     = false  # Changed to false - ALB requires minimum 2 AZs
}

variable "cloudwatch_log_retention_days" {
  description = "Number of days to retain CloudWatch logs"
  type        = number
  default     = 3
}

variable "enable_alb_access_logs" {
  description = "Enable ALB access logs"
  type        = bool
  default     = false
}

variable "cost_center" {
  description = "Cost center for resource tagging"
  type        = string
  default     = "Development"
}

variable "additional_tags" {
  description = "Additional tags to apply to all resources"
  type        = map(string)
  default = {
    Team = "Engineering"
  }
}

