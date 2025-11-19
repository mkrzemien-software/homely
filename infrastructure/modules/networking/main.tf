# Networking Module
# Creates VPC, subnets, internet gateway, and routing
#
# This module supports both single-AZ and multi-AZ configurations for cost optimization

locals {
  name_prefix = "${var.project_name}-${var.environment}"

  # Availability zones to use
  azs = var.availability_zones == 1 ? slice(data.aws_availability_zones.available.names, 0, 1) : slice(data.aws_availability_zones.available.names, 0, 2)

  # Subnet CIDR calculations
  # For single AZ: use smaller subnets from /24 VPC
  # For multi AZ: use larger subnets from /16 VPC
  public_subnet_cidrs = var.availability_zones == 1 ? [
    cidrsubnet(var.vpc_cidr, 2, 0) # 10.0.0.0/26
    ] : [
    cidrsubnet(var.vpc_cidr, 8, 1), # 10.0.1.0/24
    cidrsubnet(var.vpc_cidr, 8, 2)  # 10.0.2.0/24
  ]
}

# Data source for available AZs
data "aws_availability_zones" "available" {
  state = "available"
}

# VPC
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-vpc"
    }
  )
}

# Internet Gateway
resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-igw"
    }
  )
}

# Public Subnets
# ECS tasks will run here since we're not using NAT Gateway
resource "aws_subnet" "public" {
  count = length(local.azs)

  vpc_id                  = aws_vpc.main.id
  cidr_block              = local.public_subnet_cidrs[count.index]
  availability_zone       = local.azs[count.index]
  map_public_ip_on_launch = true

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-public-subnet-${count.index + 1}"
      Tier = "Public"
    }
  )
}

# Route Table for Public Subnets
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-public-rt"
    }
  )
}

# Route Table Associations for Public Subnets
resource "aws_route_table_association" "public" {
  count = length(aws_subnet.public)

  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

# VPC Flow Logs (optional, for debugging network issues)
# Commented out to minimize costs
# resource "aws_flow_log" "main" {
#   count                = var.enable_vpc_flow_logs ? 1 : 0
#   iam_role_arn         = aws_iam_role.vpc_flow_logs[0].arn
#   log_destination      = aws_cloudwatch_log_group.vpc_flow_logs[0].arn
#   traffic_type         = "ALL"
#   vpc_id               = aws_vpc.main.id
#
#   tags = merge(
#     var.tags,
#     {
#       Name = "${local.name_prefix}-vpc-flow-logs"
#     }
#   )
# }
