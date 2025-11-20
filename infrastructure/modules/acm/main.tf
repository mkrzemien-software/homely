# ACM Module
# Creates and validates SSL/TLS certificates for CloudFront
#
# NOTE: This module must be deployed in us-east-1 region for CloudFront certificates

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
      configuration_aliases = [aws.us_east_1]
    }
  }
}

# ACM Certificate Request
resource "aws_acm_certificate" "main" {
  provider = aws.us_east_1

  domain_name               = var.domain_name
  subject_alternative_names = var.subject_alternative_names
  validation_method         = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = merge(
    var.tags,
    {
      Name = "${var.project_name}-${var.environment}-certificate"
    }
  )
}

# Note: DNS validation records must be added manually to your DNS provider (OVH)
# or use Route53 if you manage DNS with AWS
#
# You can get the validation records from the outputs:
# - validation_records_name
# - validation_records_value
# - validation_records_type

