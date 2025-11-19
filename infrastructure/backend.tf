# Terraform Backend Configuration
#
# This configuration stores Terraform state in S3 with DynamoDB locking.
#
# IMPORTANT: Before using this backend, you must:
# 1. Create an S3 bucket for state storage
# 2. Create a DynamoDB table for state locking
# 3. Run the init.sh script to set up these resources
#
# The bucket and table names should be unique across AWS.
# They are defined in the environment-specific backend configuration.

# COMMENTED OUT BY DEFAULT
# Uncomment after running init.sh to create the S3 bucket and DynamoDB table

# terraform {
#   backend "s3" {
#     # Configuration is provided via backend config file or CLI flags
#     # See environments/dev/backend.tf and environments/prod/backend.tf
#   }
# }
