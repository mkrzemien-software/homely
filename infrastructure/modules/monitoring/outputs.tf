# Monitoring Module Outputs

output "alb_logs_bucket_name" {
  description = "Name of the S3 bucket for ALB access logs"
  value       = aws_s3_bucket.alb_logs.id
}

output "alb_logs_bucket_arn" {
  description = "ARN of the S3 bucket for ALB access logs"
  value       = aws_s3_bucket.alb_logs.arn
}

output "alb_5xx_alarm_arn" {
  description = "ARN of the ALB 5xx errors alarm"
  value       = var.enable_alarms ? aws_cloudwatch_metric_alarm.alb_5xx_errors[0].arn : null
}

output "ecs_cpu_alarm_arn" {
  description = "ARN of the ECS CPU utilization alarm"
  value       = var.enable_alarms ? aws_cloudwatch_metric_alarm.ecs_cpu_high[0].arn : null
}

output "ecs_memory_alarm_arn" {
  description = "ARN of the ECS memory utilization alarm"
  value       = var.enable_alarms ? aws_cloudwatch_metric_alarm.ecs_memory_high[0].arn : null
}
