#!/bin/bash
#
# Terraform Infrastructure Initialization Script
#
# This script sets up the prerequisites for Terraform:
# 1. Creates S3 bucket for Terraform state
# 2. Creates DynamoDB table for state locking
# 3. Enables S3 bucket versioning and encryption
#
# Usage:
#   ./init.sh <environment>
#
# Example:
#   ./init.sh dev
#   ./init.sh prod

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if environment argument is provided
if [ -z "$1" ]; then
    print_error "Environment argument is required"
    echo "Usage: ./init.sh <environment>"
    echo "Example: ./init.sh dev"
    echo "         ./init.sh prod"
    exit 1
fi

ENVIRONMENT=$1
PROJECT_NAME="homely"
AWS_REGION="us-east-1"

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
    print_error "Invalid environment: $ENVIRONMENT"
    echo "Valid environments: dev, staging, prod"
    exit 1
fi

# Configuration
STATE_BUCKET="${PROJECT_NAME}-${ENVIRONMENT}-terraform-state"
LOCK_TABLE="${PROJECT_NAME}-${ENVIRONMENT}-terraform-locks"

print_info "Initializing Terraform infrastructure for environment: $ENVIRONMENT"
print_info "AWS Region: $AWS_REGION"
print_info "S3 Bucket: $STATE_BUCKET"
print_info "DynamoDB Table: $LOCK_TABLE"
echo ""

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    print_error "AWS CLI is not installed. Please install it first."
    exit 1
fi

# Check AWS credentials
if ! aws sts get-caller-identity &> /dev/null; then
    print_error "AWS credentials are not configured or invalid"
    print_info "Run 'aws configure' to set up your credentials"
    exit 1
fi

print_info "AWS credentials validated"

# =============================================================================
# Create S3 Bucket for Terraform State
# =============================================================================

print_info "Creating S3 bucket for Terraform state..."

if aws s3 ls "s3://${STATE_BUCKET}" 2>/dev/null; then
    print_warning "S3 bucket already exists: ${STATE_BUCKET}"
else
    aws s3api create-bucket \
        --bucket "${STATE_BUCKET}" \
        --region "${AWS_REGION}" \
        --create-bucket-configuration LocationConstraint="${AWS_REGION}" 2>/dev/null || \
    aws s3api create-bucket \
        --bucket "${STATE_BUCKET}" \
        --region "${AWS_REGION}"

    print_info "✓ S3 bucket created: ${STATE_BUCKET}"
fi

# Enable versioning
print_info "Enabling versioning on S3 bucket..."
aws s3api put-bucket-versioning \
    --bucket "${STATE_BUCKET}" \
    --versioning-configuration Status=Enabled

print_info "✓ Versioning enabled"

# Enable encryption
print_info "Enabling server-side encryption..."
aws s3api put-bucket-encryption \
    --bucket "${STATE_BUCKET}" \
    --server-side-encryption-configuration '{
        "Rules": [
            {
                "ApplyServerSideEncryptionByDefault": {
                    "SSEAlgorithm": "AES256"
                }
            }
        ]
    }'

print_info "✓ Encryption enabled"

# Block public access
print_info "Blocking public access..."
aws s3api put-public-access-block \
    --bucket "${STATE_BUCKET}" \
    --public-access-block-configuration \
        BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true

print_info "✓ Public access blocked"

# =============================================================================
# Create DynamoDB Table for State Locking
# =============================================================================

print_info "Creating DynamoDB table for state locking..."

if aws dynamodb describe-table --table-name "${LOCK_TABLE}" --region "${AWS_REGION}" &>/dev/null; then
    print_warning "DynamoDB table already exists: ${LOCK_TABLE}"
else
    aws dynamodb create-table \
        --table-name "${LOCK_TABLE}" \
        --attribute-definitions AttributeName=LockID,AttributeType=S \
        --key-schema AttributeName=LockID,KeyType=HASH \
        --billing-mode PAY_PER_REQUEST \
        --region "${AWS_REGION}" \
        --tags Key=Project,Value="${PROJECT_NAME}" \
              Key=Environment,Value="${ENVIRONMENT}" \
              Key=ManagedBy,Value="Terraform"

    print_info "✓ DynamoDB table created: ${LOCK_TABLE}"

    # Wait for table to be active
    print_info "Waiting for DynamoDB table to be active..."
    aws dynamodb wait table-exists --table-name "${LOCK_TABLE}" --region "${AWS_REGION}"
    print_info "✓ DynamoDB table is active"
fi

# =============================================================================
# Summary and Next Steps
# =============================================================================

echo ""
print_info "========================================"
print_info "Terraform Backend Initialization Complete!"
print_info "========================================"
echo ""
print_info "S3 Bucket: ${STATE_BUCKET}"
print_info "DynamoDB Table: ${LOCK_TABLE}"
print_info "AWS Region: ${AWS_REGION}"
echo ""
print_info "Next steps:"
echo "  1. Update environments/${ENVIRONMENT}/backend.tf and uncomment the backend configuration"
echo "  2. Navigate to the environment directory:"
echo "     cd infrastructure/environments/${ENVIRONMENT}"
echo "  3. Initialize Terraform:"
echo "     terraform init"
echo "  4. Review the plan:"
echo "     terraform plan"
echo "  5. Apply the infrastructure:"
echo "     terraform apply"
echo ""
print_warning "IMPORTANT: Keep your terraform.tfvars file secure and never commit it to version control!"
echo ""
