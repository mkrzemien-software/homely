# Parameters Module Variables

variable "project_name" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

# Supabase Configuration
variable "supabase_url" {
  description = "Supabase project URL"
  type        = string
  sensitive   = true
  default     = ""
}

variable "supabase_anon_key" {
  description = "Supabase anonymous key"
  type        = string
  sensitive   = true
  default     = ""
}

# Application Configuration
variable "backend_log_level" {
  description = "Backend application log level (Debug, Information, Warning, Error)"
  type        = string
  default     = "Information"

  validation {
    condition     = contains(["Debug", "Information", "Warning", "Error"], var.backend_log_level)
    error_message = "Log level must be one of: Debug, Information, Warning, Error."
  }
}

variable "cors_origins" {
  description = "Allowed CORS origins (comma-separated)"
  type        = string
  default     = "*"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
