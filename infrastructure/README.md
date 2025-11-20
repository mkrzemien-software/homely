# Homely AWS Infrastructure

This directory contains Terraform Infrastructure as Code (IaC) for deploying the Homely application to AWS.

## Architecture Overview

The infrastructure includes:

- **Frontend**: Angular application hosted on S3 + CloudFront CDN
- **Backend**: .NET 9 API running on ECS Fargate (with Spot instances for cost savings)
- **Database**: Supabase Cloud (external, not managed by this infrastructure)
- **Networking**: VPC with public subnets across 2 Availability Zones (required by ALB)
- **Security**: Security Groups, IAM roles with least privilege, Parameter Store for secrets
- **Monitoring**: CloudWatch Logs (7-day retention)
- **CI/CD**: GitHub Actions with OIDC authentication

## Cost Optimization

This infrastructure is optimized for minimal cost:

- ✅ **ECS Fargate Spot**: Up to 70% savings compared to on-demand
- ✅ **2 Availability Zones**: Required by ALB, provides high availability
- ✅ **No NAT Gateway**: ECS tasks in public subnets with public IPs
- ✅ **CloudFront Free Tier**: First 1TB transfer free
- ✅ **Short log retention**: 7 days for CloudWatch logs
- ✅ **Intelligent S3 Tiering**: Automatic storage class optimization
- ✅ **Parameter Store Standard**: Free tier for secrets management

**Estimated monthly cost**: $18-35 USD for low traffic (2 AZs add ~$2-3/month)

## Prerequisites

Before deploying, ensure you have:

1. **AWS Account** with appropriate permissions
2. **AWS CLI** (v2.x or later) installed and configured
   ```bash
   aws --version
   aws configure
   ```

3. **Terraform** (v1.6 or later) installed
   ```bash
   terraform version
   ```

4. **Domain** (optional, but recommended)
   - Registered domain in OVH or any provider
   - You'll configure DNS manually after deployment

5. **Supabase Project** (optional, can be set up later)
   - Supabase URL
   - Supabase Anonymous Key

## Directory Structure

```
infrastructure/
├── main.tf                    # Main configuration
├── variables.tf               # Input variables
├── outputs.tf                 # Output values
├── providers.tf               # AWS provider configuration
├── backend.tf                 # Remote state configuration
├── terraform.tfvars.example   # Example variables file
├── init.sh                    # Initialization script
├── README.md                  # This file
│
├── modules/
│   ├── networking/            # VPC, subnets, IGW
│   ├── frontend/              # S3, CloudFront
│   ├── backend/               # ECS, ALB, ECR
│   ├── security/              # Security Groups, IAM
│   ├── parameters/            # Parameter Store
│   └── monitoring/            # CloudWatch, alarms
│
└── environments/
    ├── dev/                   # Development environment
    │   ├── main.tf
    │   ├── backend.tf
    │   └── terraform.tfvars.example
    └── prod/                  # Production environment
        ├── main.tf
        ├── backend.tf
        └── terraform.tfvars.example
```

## Quick Start

### 1. Initialize Backend (One-time setup)

The initialization script creates S3 bucket and DynamoDB table for Terraform state:

```bash
# Make the script executable
chmod +x init.sh

# Run for dev environment
./init.sh dev

# Run for prod environment
./init.sh prod
```

### 2. Configure Variables

Copy and customize the variables file:

```bash
# For dev environment
cd environments/dev
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values

# For prod environment
cd environments/prod
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
```

**Important placeholders to replace**:
- `domain_name`: Your actual domain (e.g., "example.com")
- `github_repo`: Your GitHub repository (e.g., "username/homely")

### 3. Enable Remote Backend

After running `init.sh`, uncomment the backend configuration in `environments/{env}/backend.tf`:

```hcl
terraform {
  backend "s3" {
    bucket         = "homely-dev-terraform-state"
    key            = "dev/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "homely-dev-terraform-locks"
  }
}
```

### 4. Deploy Infrastructure

```bash
# Navigate to environment directory
cd environments/dev  # or environments/prod

# Initialize Terraform (downloads providers and modules)
terraform init

# Review the execution plan
terraform plan

# Apply the infrastructure
terraform apply

# When prompted, type 'yes' to confirm
```

### 5. Configure DNS (OVH)

After deployment, Terraform will output DNS configuration instructions. Add these records in OVH:

```
Frontend (CloudFront):
Type: CNAME
Name: app  (or dev-app for dev)
Target: d123456abcdef.cloudfront.net
TTL: 3600

Backend (ALB):
Type: CNAME
Name: api  (or dev-api for dev)
Target: homely-prod-alb-123456789.us-east-1.elb.amazonaws.com
TTL: 3600
```

### 6. SSL Certificates (Optional but Recommended)

To enable HTTPS with custom domain:

1. **Request certificate in ACM**:
   ```bash
   # For CloudFront (must be in us-east-1)
   aws acm request-certificate \
     --domain-name "*.yourdomain.com" \
     --validation-method DNS \
     --region us-east-1

   # For ALB (in your deployment region)
   aws acm request-certificate \
     --domain-name "*.yourdomain.com" \
     --validation-method DNS \
     --region us-east-1
   ```

2. **Add DNS validation records** from ACM console to OVH

3. **Update Terraform configuration** with certificate ARNs in `main.tf`

4. **Re-apply Terraform**:
   ```bash
   terraform apply
   ```

### 7. Set Parameter Store Values

Set your Supabase credentials and other configuration:

```bash
# Get the commands from Terraform outputs
terraform output -raw parameter_store_setup_commands

# Or set manually:
aws ssm put-parameter \
  --name "/homely/prod/supabase_url" \
  --value "https://xxxxx.supabase.co" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite

aws ssm put-parameter \
  --name "/homely/prod/supabase_anon_key" \
  --value "your-anon-key-here" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite
```

### 8. Configure GitHub Actions

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

**For OIDC Authentication (Recommended)**:
```
AWS_ROLE_ARN=arn:aws:iam::123456789012:role/homely-prod-github-actions-role
```

Get the role ARN from Terraform outputs:
```bash
terraform output | grep github_actions_role_arn
```

**Environment-specific secrets**:
```bash
# Production
FRONTEND_S3_BUCKET_PROD=homely-prod-frontend
CLOUDFRONT_DISTRIBUTION_ID_PROD=E123456789ABCD
ECR_REPOSITORY_URL_PROD=123456789012.dkr.ecr.us-east-1.amazonaws.com/homely-prod-backend
ECS_CLUSTER_NAME_PROD=homely-prod-cluster
ECS_SERVICE_NAME_PROD=homely-prod-service

# Development
FRONTEND_S3_BUCKET_DEV=homely-dev-frontend
CLOUDFRONT_DISTRIBUTION_ID_DEV=E987654321DCBA
ECR_REPOSITORY_URL_DEV=123456789012.dkr.ecr.us-east-1.amazonaws.com/homely-dev-backend
ECS_CLUSTER_NAME_DEV=homely-dev-cluster
ECS_SERVICE_NAME_DEV=homely-dev-service
```

**Supabase database secrets** (for migration workflow):
```bash
SUPABASE_ACCESS_TOKEN=sbp_your_access_token
SUPABASE_PROJECT_REF_DEV=your-dev-project-ref
SUPABASE_PROJECT_REF_PROD=your-prod-project-ref
SUPABASE_DB_PASSWORD_DEV=your-dev-db-password
SUPABASE_DB_PASSWORD_PROD=your-prod-db-password
```

Get AWS values from Terraform outputs:
```bash
terraform output
```

For Supabase setup, see [database/SUPABASE_SETUP.md](../database/SUPABASE_SETUP.md)

### 9. Deploy Infrastructure and Application

#### Deploy Infrastructure (Terraform)

Use GitHub Actions workflow for safe infrastructure deployment:

**Via GitHub Actions UI:**
1. Go to Actions → "Deploy Infrastructure"
2. Click "Run workflow"
3. Select options:
   - **Environment**: dev or prod
   - **Only run terraform plan**: ✅ Check for dry-run (recommended first!)
   - **Auto-approve apply**: ⚠️ Only for dev or after reviewing plan
4. Review plan output in workflow logs
5. If plan looks good, run again with "Only plan" unchecked

**Workflow features:**
- ✅ Dry-run mode (plan only) - safe to run anytime
- ✅ Requires manual confirmation for prod (unless auto-approve checked)
- ✅ Uploads plan output as artifact (30 days retention)
- ✅ Saves Terraform outputs as artifact (90 days retention)
- ✅ Format validation before apply

**Local deployment (alternative):**
```bash
cd infrastructure/environments/dev  # or prod

# Review changes first
terraform plan

# Apply changes
terraform apply
```

#### Deploy Application

Push to main/master branch or trigger workflow manually:

```bash
# Database migrations
# Triggers automatically when database/supabase/migrations/ files change

# Frontend deployment
# Triggers automatically when frontend/ files change

# Backend deployment
# Triggers automatically when backend/ files change

# Or manually via GitHub Actions UI:
# - Deploy Infrastructure (dev/prod with plan-only option) ← NEW!
# - Deploy Database Migrations (dev/prod with dry-run option)
# - Deploy Frontend (dev/prod)
# - Deploy Backend (dev/prod)
```

## Terraform Commands Reference

```bash
# Initialize Terraform
terraform init

# Format code
terraform fmt -recursive

# Validate configuration
terraform validate

# Plan changes
terraform plan

# Apply changes
terraform apply

# Show current state
terraform show

# List resources
terraform state list

# Output values
terraform output

# Destroy infrastructure (⚠️ CAREFUL!)
terraform destroy
```

## Troubleshooting

### Issue: "Error creating S3 bucket"

**Cause**: Bucket name must be globally unique

**Solution**: Change `project_name` in `terraform.tfvars` to something unique

### Issue: "Error assuming role"

**Cause**: AWS credentials not configured or insufficient permissions

**Solution**:
```bash
aws configure
aws sts get-caller-identity
```

### Issue: "ECS tasks failing health checks"

**Cause**: Application not starting correctly or health endpoint not responding

**Solution**:
1. Check CloudWatch logs:
   ```bash
   aws logs tail /ecs/homely-prod-backend --follow
   ```

2. Verify Parameter Store values are set correctly

3. Check security groups allow traffic

### Issue: "CloudFront returns 403 Forbidden"

**Cause**: S3 bucket policy not allowing CloudFront OAC

**Solution**: Re-apply Terraform to fix bucket policy:
```bash
terraform apply
```

### Issue: "GitHub Actions deployment fails"

**Cause**: Missing secrets or incorrect OIDC role

**Solution**:
1. Verify all GitHub secrets are set correctly
2. Check IAM role trust policy allows your repository
3. Review GitHub Actions logs for specific errors

## Updating Infrastructure

To update existing infrastructure:

1. **Modify Terraform files** as needed

2. **Review changes**:
   ```bash
   terraform plan
   ```

3. **Apply changes**:
   ```bash
   terraform apply
   ```

4. **For sensitive changes**, consider:
   - Blue-green deployments
   - Gradual rollouts
   - Testing in dev first

## Rollback Strategy

If deployment fails:

### Terraform Rollback

```bash
# View state history
terraform state list

# Restore from backup (if using S3 backend versioning)
aws s3api list-object-versions --bucket homely-prod-terraform-state --prefix prod/

# Restore specific version
aws s3api get-object \
  --bucket homely-prod-terraform-state \
  --key prod/terraform.tfstate \
  --version-id <version-id> \
  terraform.tfstate

# Re-apply with restored state
terraform apply
```

### Application Rollback

```bash
# List previous task definitions
aws ecs list-task-definitions \
  --family-prefix homely-prod-backend \
  --sort DESC

# Update service to previous task definition
aws ecs update-service \
  --cluster homely-prod-cluster \
  --service homely-prod-service \
  --task-definition homely-prod-backend:123  # Previous revision
```

## Backup and Disaster Recovery

### Terraform State Backups

- **S3 versioning enabled**: Automatic backups of state files
- **Manual backup**:
  ```bash
  terraform state pull > terraform.tfstate.backup
  ```

### Infrastructure Recreation

If infrastructure is destroyed, you can recreate it:

```bash
# Pull latest code
git pull origin main

# Navigate to environment
cd infrastructure/environments/prod

# Re-initialize and apply
terraform init
terraform apply
```

## Security Best Practices

1. **Never commit secrets**:
   - Use Parameter Store for secrets
   - Never put secrets in `terraform.tfvars`
   - Add `terraform.tfvars` to `.gitignore`

2. **Use OIDC for GitHub Actions**:
   - Avoid using AWS access keys
   - Rotate credentials regularly

3. **Enable MFA for AWS**:
   - Especially for production account

4. **Review IAM policies**:
   - Follow principle of least privilege
   - Regularly audit permissions

5. **Monitor CloudWatch Logs**:
   - Set up alarms for errors
   - Review logs regularly

## Cost Monitoring

Monitor your AWS costs:

```bash
# Get current month costs
aws ce get-cost-and-usage \
  --time-period Start=2024-01-01,End=2024-01-31 \
  --granularity MONTHLY \
  --metrics UnblendedCost \
  --group-by Type=SERVICE
```

**Cost optimization tips**:
- Use Fargate Spot (up to 70% savings)
- Stop dev environment when not in use
- Monitor CloudWatch Logs retention
- Review unused resources regularly

## Additional Resources

- [Terraform AWS Provider Documentation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [AWS ECS Best Practices](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/)
- [GitHub Actions OIDC with AWS](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)

## Support

For issues or questions:
1. Check Troubleshooting section above
2. Review CloudWatch Logs
3. Check GitHub Issues
4. Contact DevOps team

---

**Last Updated**: 2025-01-19
**Terraform Version**: >= 1.6
**AWS Provider Version**: ~> 5.0
