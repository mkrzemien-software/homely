# AWS Provider Configuration

terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = merge(
      {
        Project     = var.project_name
        Environment = var.environment
        ManagedBy   = "Terraform"
        CostCenter  = var.cost_center
      },
      var.additional_tags
    )
  }
}

# Provider alias for us-east-1 (required for ACM certificates with CloudFront)
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"

  default_tags {
    tags = merge(
      {
        Project     = var.project_name
        Environment = var.environment
        ManagedBy   = "Terraform"
        CostCenter  = var.cost_center
      },
      var.additional_tags
    )
  }
}
