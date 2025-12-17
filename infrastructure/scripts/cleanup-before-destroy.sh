#!/bin/bash
# Cleanup script to run BEFORE terraform destroy
# This script empties S3 buckets and ECR repositories so Terraform can delete them

set -e  # Exit on error

ENVIRONMENT="${1:-dev}"
REGION="${2:-us-east-1}"

echo "================================================"
echo "🧹 Pre-Destroy Cleanup Script"
echo "================================================"
echo ""
echo "Environment: $ENVIRONMENT"
echo "Region: $REGION"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to empty S3 bucket
empty_s3_bucket() {
    local bucket_name=$1

    echo -e "${YELLOW}🗑️  Emptying S3 bucket: $bucket_name${NC}"

    # Check if bucket exists
    if aws s3 ls "s3://$bucket_name" 2>/dev/null; then
        echo "  - Deleting all objects..."
        aws s3 rm "s3://$bucket_name" --recursive --region "$REGION" || true

        echo "  - Deleting all object versions..."
        aws s3api delete-objects \
            --bucket "$bucket_name" \
            --delete "$(aws s3api list-object-versions \
                --bucket "$bucket_name" \
                --output json \
                --query '{Objects: Versions[].{Key:Key,VersionId:VersionId}}' \
                --region "$REGION")" \
            --region "$REGION" 2>/dev/null || true

        echo "  - Deleting all delete markers..."
        aws s3api delete-objects \
            --bucket "$bucket_name" \
            --delete "$(aws s3api list-object-versions \
                --bucket "$bucket_name" \
                --output json \
                --query '{Objects: DeleteMarkers[].{Key:Key,VersionId:VersionId}}' \
                --region "$REGION")" \
            --region "$REGION" 2>/dev/null || true

        echo -e "${GREEN}  ✅ Bucket $bucket_name is now empty${NC}"
    else
        echo -e "${YELLOW}  ⚠️  Bucket $bucket_name not found (may already be deleted)${NC}"
    fi
    echo ""
}

# Function to empty ECR repository
empty_ecr_repository() {
    local repo_name=$1

    echo -e "${YELLOW}🗑️  Emptying ECR repository: $repo_name${NC}"

    # Check if repository exists
    if aws ecr describe-repositories --repository-names "$repo_name" --region "$REGION" >/dev/null 2>&1; then
        # Get all image IDs
        IMAGE_IDS=$(aws ecr list-images \
            --repository-name "$repo_name" \
            --region "$REGION" \
            --query 'imageIds[*]' \
            --output json)

        # Count images
        IMAGE_COUNT=$(echo "$IMAGE_IDS" | jq '. | length')

        if [ "$IMAGE_COUNT" -gt 0 ]; then
            echo "  - Deleting $IMAGE_COUNT image(s)..."
            aws ecr batch-delete-image \
                --repository-name "$repo_name" \
                --image-ids "$IMAGE_IDS" \
                --region "$REGION" >/dev/null
            echo -e "${GREEN}  ✅ Deleted $IMAGE_COUNT image(s) from $repo_name${NC}"
        else
            echo -e "${GREEN}  ✅ Repository $repo_name is already empty${NC}"
        fi
    else
        echo -e "${YELLOW}  ⚠️  Repository $repo_name not found (may already be deleted)${NC}"
    fi
    echo ""
}

# Start cleanup
echo "Starting cleanup process..."
echo ""

# S3 Buckets
empty_s3_bucket "homely-${ENVIRONMENT}-frontend"
empty_s3_bucket "homely-${ENVIRONMENT}-cloudfront-logs"

# ECR Repository
empty_ecr_repository "homely-${ENVIRONMENT}-backend"

echo "================================================"
echo -e "${GREEN}✅ CLEANUP COMPLETED${NC}"
echo "================================================"
echo ""
echo "All resources have been emptied and are ready for deletion."
echo ""
echo "Next steps:"
echo "1. Run: terraform destroy"
echo "   or"
echo "2. Use GitHub Actions 'Destroy Infrastructure' workflow"
echo ""
