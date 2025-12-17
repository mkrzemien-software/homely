# Backend Module
# Creates ECR repository, ECS Fargate cluster, ALB, and related resources

locals {
  name_prefix      = "${var.project_name}-${var.environment}"
  container_name   = "${local.name_prefix}-api"
  ecr_repository   = "${local.name_prefix}-backend"
  log_group_name   = "/ecs/${local.name_prefix}-backend"
}

# ECR Repository for Docker Images
resource "aws_ecr_repository" "backend" {
  name                 = local.ecr_repository
  image_tag_mutability = "MUTABLE"
  force_delete         = true # Allow deletion even if repository contains images

  image_scanning_configuration {
    scan_on_push = true # Free basic scanning
  }

  encryption_configuration {
    encryption_type = "AES256" # Free encryption
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecr"
    }
  )
}

# ECR Lifecycle Policy (keep last 10 images to save storage costs)
resource "aws_ecr_lifecycle_policy" "backend" {
  repository = aws_ecr_repository.backend.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 10 images"
        selection = {
          tagStatus     = "any"
          countType     = "imageCountMoreThan"
          countNumber   = 10
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

# CloudWatch Log Group for ECS Tasks
resource "aws_cloudwatch_log_group" "backend" {
  name              = local.log_group_name
  retention_in_days = var.cloudwatch_log_retention_days

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-backend-logs"
    }
  )
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "${local.name_prefix}-cluster"

  setting {
    name  = "containerInsights"
    value = "disabled" # Disable to save costs
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-cluster"
    }
  )
}

# ECS Cluster Capacity Providers (Fargate Spot for cost savings)
resource "aws_ecs_cluster_capacity_providers" "main" {
  cluster_name = aws_ecs_cluster.main.name

  capacity_providers = var.use_fargate_spot ? ["FARGATE_SPOT", "FARGATE"] : ["FARGATE"]

  default_capacity_provider_strategy {
    capacity_provider = var.use_fargate_spot ? "FARGATE_SPOT" : "FARGATE"
    weight            = 100
    base              = 1
  }

  # Fallback to FARGATE if FARGATE_SPOT is interrupted
  dynamic "default_capacity_provider_strategy" {
    for_each = var.use_fargate_spot ? [1] : []
    content {
      capacity_provider = "FARGATE"
      weight            = 0
      base              = 0
    }
  }
}

# ECS Task Definition
resource "aws_ecs_task_definition" "backend" {
  family                   = "${local.name_prefix}-backend"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.task_cpu
  memory                   = var.task_memory
  execution_role_arn       = var.ecs_task_execution_role_arn
  task_role_arn            = var.ecs_task_role_arn

  container_definitions = jsonencode([
    {
      name      = local.container_name
      image     = "${aws_ecr_repository.backend.repository_url}:${var.docker_image_tag}"
      essential = true

      portMappings = [
        {
          containerPort = var.container_port
          protocol      = "tcp"
        }
      ]

      # Environment variables from Parameter Store
      secrets = [
        {
          name      = "Supabase__Url"
          valueFrom = var.parameter_store_paths.supabase_url
        },
        {
          name      = "Supabase__Key"
          valueFrom = var.parameter_store_paths.supabase_anon_key
        },
        {
          name      = "Supabase__ServiceRoleKey"
          valueFrom = var.parameter_store_paths.supabase_service_role_key
        },
        {
          name      = "ConnectionStrings__DefaultConnection"
          valueFrom = var.parameter_store_paths.database_connection_string
        },
        {
          name      = "JwtSettings__ValidIssuer"
          valueFrom = var.parameter_store_paths.jwt_valid_issuer
        },
        {
          name      = "JwtSettings__ValidAudience"
          valueFrom = var.parameter_store_paths.jwt_valid_audience
        },
        {
          name      = "JwtSettings__Secret"
          valueFrom = var.parameter_store_paths.jwt_secret
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = var.environment == "prod" ? "Production" : "Development"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:${var.container_port}"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = local.log_group_name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:${var.container_port}${var.health_check_path} || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-backend-task"
    }
  )
}

# Application Load Balancer
resource "aws_lb" "backend" {
  name               = "${local.name_prefix}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [var.alb_security_group_id]
  subnets            = var.public_subnet_ids

  enable_deletion_protection = false
  enable_http2               = true
  enable_cross_zone_load_balancing = true

  # ALB Access Logs (optional, disabled by default to save costs)
  dynamic "access_logs" {
    for_each = var.enable_alb_access_logs ? [1] : []
    content {
      bucket  = var.alb_access_logs_bucket
      enabled = true
      prefix  = "alb"
    }
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-alb"
    }
  )
}

# ALB Target Group
resource "aws_lb_target_group" "backend" {
  name_prefix = "hml-"  # AWS will add unique suffix (max 6 chars for prefix)
  port        = var.container_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = var.health_check_path
    protocol            = "HTTP"
    matcher             = "200"
  }

  deregistration_delay = 30

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-tg"
    }
  )
}

# ALB Listener (HTTP)
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.backend.arn
  port              = "80"
  protocol          = "HTTP"

  # Redirect HTTP to HTTPS if certificate is provided
  default_action {
    type = var.acm_certificate_arn != "" ? "redirect" : "forward"

    dynamic "redirect" {
      for_each = var.acm_certificate_arn != "" ? [1] : []
      content {
        port        = "443"
        protocol    = "HTTPS"
        status_code = "HTTP_301"
      }
    }

    target_group_arn = var.acm_certificate_arn == "" ? aws_lb_target_group.backend.arn : null
  }
}

# ALB Listener (HTTPS) - Only if ACM certificate is provided
resource "aws_lb_listener" "https" {
  count = var.acm_certificate_arn != "" ? 1 : 0

  load_balancer_arn = aws_lb.backend.arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.acm_certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}

# ECS Service
resource "aws_ecs_service" "backend" {
  name            = "${local.name_prefix}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.backend.arn
  desired_count   = var.desired_count
  launch_type     = var.use_fargate_spot ? null : "FARGATE"

  # Use capacity provider strategy if Fargate Spot is enabled
  dynamic "capacity_provider_strategy" {
    for_each = var.use_fargate_spot ? [1] : []
    content {
      capacity_provider = "FARGATE_SPOT"
      weight            = 100
      base              = 1
    }
  }

  network_configuration {
    subnets          = var.public_subnet_ids
    security_groups  = [var.ecs_security_group_id]
    assign_public_ip = true # Required for internet access without NAT Gateway
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.backend.arn
    container_name   = local.container_name
    container_port   = var.container_port
  }

  # Allow external changes to desired_count (for CI/CD)
  # Note: task_definition is NOT ignored during infrastructure changes
  lifecycle {
    ignore_changes = [desired_count]
  }

  # Wait for ALB to be ready
  depends_on = [aws_lb_listener.http]

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-ecs-service"
    }
  )
}
