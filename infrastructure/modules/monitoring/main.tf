# Monitoring Module
# Creates CloudWatch alarms and S3 bucket for ALB access logs

locals {
  name_prefix         = "${var.project_name}-${var.environment}"
  alb_logs_bucket_name = "${local.name_prefix}-alb-logs"
}

# Data source for ALB logs service account (region-specific)
data "aws_elb_service_account" "main" {}

# =============================================================================
# S3 Bucket for ALB Access Logs
# =============================================================================

resource "aws_s3_bucket" "alb_logs" {
  bucket = local.alb_logs_bucket_name

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-alb-logs"
    }
  )
}

# S3 Bucket Public Access Block
resource "aws_s3_bucket_public_access_block" "alb_logs" {
  bucket = aws_s3_bucket.alb_logs.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# S3 Bucket Lifecycle Configuration
resource "aws_s3_bucket_lifecycle_configuration" "alb_logs" {
  bucket = aws_s3_bucket.alb_logs.id

  rule {
    id     = "delete-old-logs"
    status = "Enabled"

    filter {}

    expiration {
      days = var.log_retention_days
    }
  }
}

# S3 Bucket Policy for ALB
resource "aws_s3_bucket_policy" "alb_logs" {
  bucket = aws_s3_bucket.alb_logs.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowALBLogs"
        Effect = "Allow"
        Principal = {
          AWS = data.aws_elb_service_account.main.arn
        }
        Action   = "s3:PutObject"
        Resource = "${aws_s3_bucket.alb_logs.arn}/*"
      }
    ]
  })
}

# =============================================================================
# CloudWatch Alarms (Optional, for cost optimization)
# =============================================================================

# CloudWatch Alarm for ALB 5xx Errors
resource "aws_cloudwatch_metric_alarm" "alb_5xx_errors" {
  count = var.enable_alarms ? 1 : 0

  alarm_name          = "${local.name_prefix}-alb-5xx-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "HTTPCode_Target_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  period              = 60
  statistic           = "Sum"
  threshold           = 10
  alarm_description   = "This alarm monitors ALB 5xx errors"
  treat_missing_data  = "notBreaching"

  dimensions = {
    LoadBalancer = var.alb_arn
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-alb-5xx-errors-alarm"
    }
  )
}

# CloudWatch Alarm for ECS Task Failures
resource "aws_cloudwatch_metric_alarm" "ecs_task_failures" {
  count = var.enable_alarms ? 1 : 0

  alarm_name          = "${local.name_prefix}-ecs-task-failures"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "DesiredTaskCount"
  namespace           = "AWS/ECS"
  period              = 60
  statistic           = "Average"
  threshold           = 0
  alarm_description   = "This alarm monitors ECS task failures"
  treat_missing_data  = "notBreaching"

  dimensions = {
    ClusterName = var.ecs_cluster_name
    ServiceName = var.ecs_service_name
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-task-failures-alarm"
    }
  )
}

# CloudWatch Alarm for ECS CPU Utilization (high usage)
resource "aws_cloudwatch_metric_alarm" "ecs_cpu_high" {
  count = var.enable_alarms ? 1 : 0

  alarm_name          = "${local.name_prefix}-ecs-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "This alarm monitors ECS CPU utilization"
  treat_missing_data  = "notBreaching"

  dimensions = {
    ClusterName = var.ecs_cluster_name
    ServiceName = var.ecs_service_name
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-cpu-high-alarm"
    }
  )
}

# CloudWatch Alarm for ECS Memory Utilization (high usage)
resource "aws_cloudwatch_metric_alarm" "ecs_memory_high" {
  count = var.enable_alarms ? 1 : 0

  alarm_name          = "${local.name_prefix}-ecs-memory-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "MemoryUtilization"
  namespace           = "AWS/ECS"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "This alarm monitors ECS memory utilization"
  treat_missing_data  = "notBreaching"

  dimensions = {
    ClusterName = var.ecs_cluster_name
    ServiceName = var.ecs_service_name
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-memory-high-alarm"
    }
  )
}
