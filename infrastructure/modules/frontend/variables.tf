# Frontend Module Variables

variable "project_name" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "domain_name" {
  description = "Domain name for the frontend (e.g., app.example.com)"
  type        = string
}

variable "acm_certificate_arn" {
  description = "ARN of ACM certificate for CloudFront (must be in us-east-1). Leave empty to use CloudFront default certificate."
  type        = string
  default     = ""
}

variable "cors_allowed_origins" {
  description = "Allowed origins for CORS"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
