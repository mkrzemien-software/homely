# Backend Module Outputs

# ECR Outputs
output "ecr_repository_url" {
  description = "URL of the ECR repository"
  value       = aws_ecr_repository.backend.repository_url
}

output "ecr_repository_arn" {
  description = "ARN of the ECR repository"
  value       = aws_ecr_repository.backend.arn
}

output "ecr_repository_name" {
  description = "Name of the ECR repository"
  value       = aws_ecr_repository.backend.name
}

# ECS Outputs
output "ecs_cluster_id" {
  description = "ID of the ECS cluster"
  value       = aws_ecs_cluster.main.id
}

output "ecs_cluster_arn" {
  description = "ARN of the ECS cluster"
  value       = aws_ecs_cluster.main.arn
}

output "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  value       = aws_ecs_cluster.main.name
}

output "ecs_service_id" {
  description = "ID of the ECS service"
  value       = aws_ecs_service.backend.id
}

output "ecs_service_name" {
  description = "Name of the ECS service"
  value       = aws_ecs_service.backend.name
}

output "ecs_task_definition_arn" {
  description = "ARN of the ECS task definition"
  value       = aws_ecs_task_definition.backend.arn
}

# ALB Outputs
output "alb_id" {
  description = "ID of the Application Load Balancer"
  value       = aws_lb.backend.id
}

output "alb_arn" {
  description = "ARN of the Application Load Balancer"
  value       = aws_lb.backend.arn
}

output "alb_dns_name" {
  description = "DNS name of the Application Load Balancer"
  value       = aws_lb.backend.dns_name
}

output "alb_zone_id" {
  description = "Zone ID of the Application Load Balancer"
  value       = aws_lb.backend.zone_id
}

output "alb_target_group_arn" {
  description = "ARN of the ALB target group"
  value       = aws_lb_target_group.backend.arn
}

# CloudWatch Outputs
output "cloudwatch_log_group_name" {
  description = "Name of the CloudWatch log group"
  value       = aws_cloudwatch_log_group.backend.name
}

output "cloudwatch_log_group_arn" {
  description = "ARN of the CloudWatch log group"
  value       = aws_cloudwatch_log_group.backend.arn
}
