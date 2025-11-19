# Networking Module Variables

variable "project_name" {
  description = "Name of the project"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "aws_region" {
  description = "AWS region"
  type        = string
}

variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "Number of availability zones to use (1 or 2)"
  type        = number
  default     = 2

  validation {
    condition     = var.availability_zones == 1 || var.availability_zones == 2
    error_message = "Availability zones must be 1 or 2."
  }
}

variable "use_single_az" {
  description = "Use single availability zone for cost savings"
  type        = bool
  default     = false
}

variable "enable_vpc_flow_logs" {
  description = "Enable VPC flow logs (additional CloudWatch costs)"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
