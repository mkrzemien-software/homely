# Security Module Variables

variable "project_name" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "vpc_id" {
  description = "VPC ID for security groups"
  type        = string
}

variable "github_repo" {
  description = "GitHub repository name (owner/repo) for OIDC trust"
  type        = string
}

variable "github_oidc_thumbprint" {
  description = "Thumbprint for GitHub OIDC provider"
  type        = string
  default     = "6938fd4d98bab03faadb97b34396831e3780aea1"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
