# Security Module Outputs

# Security Groups
output "alb_security_group_id" {
  description = "ID of the ALB security group"
  value       = aws_security_group.alb.id
}

output "ecs_security_group_id" {
  description = "ID of the ECS tasks security group"
  value       = aws_security_group.ecs_tasks.id
}

# IAM Roles for ECS
output "ecs_task_execution_role_arn" {
  description = "ARN of the ECS task execution role"
  value       = aws_iam_role.ecs_task_execution.arn
}

output "ecs_task_execution_role_name" {
  description = "Name of the ECS task execution role"
  value       = aws_iam_role.ecs_task_execution.name
}

output "ecs_task_role_arn" {
  description = "ARN of the ECS task role"
  value       = aws_iam_role.ecs_task.arn
}

output "ecs_task_role_name" {
  description = "Name of the ECS task role"
  value       = aws_iam_role.ecs_task.name
}

# IAM Role for GitHub Actions
output "github_actions_role_arn" {
  description = "ARN of the GitHub Actions role"
  value       = aws_iam_role.github_actions.arn
}

output "github_actions_role_name" {
  description = "Name of the GitHub Actions role"
  value       = aws_iam_role.github_actions.name
}

output "github_oidc_provider_arn" {
  description = "ARN of the GitHub OIDC provider"
  value       = aws_iam_openid_connect_provider.github.arn
}
