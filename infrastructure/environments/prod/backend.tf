# Production Environment Backend Configuration
#
# IMPORTANT: Before using this backend, run the init.sh script to create
# the S3 bucket and DynamoDB table for state management
#
# Uncomment after running init.sh

# terraform {
#   backend "s3" {
#     bucket         = "homely-prod-terraform-state"
#     key            = "prod/terraform.tfstate"
#     region         = "us-east-1"
#     encrypt        = true
#     dynamodb_table = "homely-prod-terraform-locks"
#   }
# }
