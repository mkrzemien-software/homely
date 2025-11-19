# Monitoring Module Variables

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

variable "log_retention_days" {
  description = "Number of days to retain logs in S3"
  type        = number
  default     = 7
}

variable "enable_alarms" {
  description = "Enable CloudWatch alarms (additional costs)"
  type        = bool
  default     = false
}

# ECS Configuration
variable "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  type        = string
}

variable "ecs_service_name" {
  description = "Name of the ECS service"
  type        = string
}

# ALB Configuration
variable "alb_arn" {
  description = "ARN of the Application Load Balancer"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
