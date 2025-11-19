# Supabase Setup Guide

This guide explains how to set up separate Supabase projects for development and production environments and configure automated migrations via GitHub Actions.

## Prerequisites

- Supabase account (https://supabase.com)
- Supabase CLI installed locally
- GitHub repository with Actions enabled

## 1. Create Supabase Projects

### Create Development Project

1. Go to https://app.supabase.com
2. Click "New project"
3. Fill in project details:
   - **Name**: `homely-dev`
   - **Database Password**: Generate a strong password (save it!)
   - **Region**: Choose closest to your users (e.g., `us-east-1`)
4. Click "Create new project"
5. Wait for project to be ready (~2 minutes)
6. **Save the following** from Project Settings → API:
   - Project URL: `https://xxxxx.supabase.co`
   - Project API URL: `https://xxxxx.supabase.co`
   - Project Reference ID: `xxxxx` (from the URL)
   - `anon` public key
   - `service_role` secret key (keep this secure!)

### Create Production Project

Repeat the same steps but use:
- **Name**: `homely-prod`
- Use a **different** strong database password
- Same region (for consistency)

**Important**: Keep dev and prod projects completely separate!

## 2. Link Local Development

```bash
# Navigate to database directory
cd database

# Install Supabase CLI (if not already installed)
npm install

# Link to development project
npx supabase login
npx supabase link --project-ref <your-dev-project-ref>

# Verify connection
npx supabase status
```

## 3. Generate Access Token

To allow GitHub Actions to push migrations, you need a Supabase access token:

1. Go to https://app.supabase.com/account/tokens
2. Click "Generate new token"
3. Name: `GitHub Actions - Homely`
4. Click "Generate token"
5. **Copy the token immediately** (you won't see it again!)

This token works for **all** your Supabase projects.

## 4. Configure GitHub Secrets

Add the following secrets to your GitHub repository:

**Settings → Secrets and variables → Actions → New repository secret**

### Required Secrets

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SUPABASE_ACCESS_TOKEN` | Personal access token from Supabase | `sbp_abc123...` |
| `SUPABASE_PROJECT_REF_DEV` | Dev project reference ID | `abcdefghijklmnop` |
| `SUPABASE_PROJECT_REF_PROD` | Prod project reference ID | `qrstuvwxyzabcdef` |
| `SUPABASE_DB_PASSWORD_DEV` | Dev database password | `your-dev-password` |
| `SUPABASE_DB_PASSWORD_PROD` | Prod database password | `your-prod-password` |

### How to Add Secrets

```bash
# Example using GitHub CLI
gh secret set SUPABASE_ACCESS_TOKEN -b "sbp_your_token_here"
gh secret set SUPABASE_PROJECT_REF_DEV -b "your-dev-ref"
gh secret set SUPABASE_PROJECT_REF_PROD -b "your-prod-ref"
gh secret set SUPABASE_DB_PASSWORD_DEV -b "your-dev-password"
gh secret set SUPABASE_DB_PASSWORD_PROD -b "your-prod-password"
```

Or manually via GitHub web interface:
1. Go to repository Settings
2. Secrets and variables → Actions
3. Click "New repository secret"
4. Add each secret one by one

## 5. Update AWS Parameter Store

Update the Supabase URLs and keys in AWS Parameter Store for each environment:

```bash
# Development environment
aws ssm put-parameter \
  --name "/homely/dev/supabase_url" \
  --value "https://xxxxx.supabase.co" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite

aws ssm put-parameter \
  --name "/homely/dev/supabase_anon_key" \
  --value "your-dev-anon-key" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite

# Production environment
aws ssm put-parameter \
  --name "/homely/prod/supabase_url" \
  --value "https://yyyyy.supabase.co" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite

aws ssm put-parameter \
  --name "/homely/prod/supabase_anon_key" \
  --value "your-prod-anon-key" \
  --type "SecureString" \
  --region us-east-1 \
  --overwrite
```

## 6. Initial Migration Setup

### Apply Existing Migrations to Dev

```bash
cd database

# Make sure you're linked to dev project
npx supabase link --project-ref <dev-project-ref>

# Push all existing migrations
npx supabase db push

# Verify migrations were applied
npx supabase migration list
```

### Apply Migrations to Prod

```bash
# Link to prod project
npx supabase link --project-ref <prod-project-ref>

# Push migrations to production
npx supabase db push

# Verify
npx supabase migration list
```

## 7. Workflow Usage

### Automatic Deployment (on push)

Migrations are automatically deployed to **dev** when you push changes to `database/supabase/migrations/`:

```bash
# Create a new migration
cd database
npx supabase migration new add_new_feature

# Edit the migration file
# Commit and push
git add .
git commit -m "Add new database feature"
git push origin main

# GitHub Actions will automatically apply to dev environment
```

### Manual Deployment

#### Deploy to Dev (with dry run first)

1. Go to GitHub Actions
2. Select "Deploy Database Migrations" workflow
3. Click "Run workflow"
4. Choose:
   - Environment: `dev`
   - Dry run: `true` (to test first)
5. Click "Run workflow"
6. Review the output
7. Run again with dry run: `false` to apply

#### Deploy to Production

**⚠️ IMPORTANT**: Always test in dev first!

1. Verify migrations work in dev
2. Go to GitHub Actions
3. Select "Deploy Database Migrations" workflow
4. Click "Run workflow"
5. Choose:
   - Environment: `prod`
   - Dry run: `false`
6. Click "Run workflow"
7. Monitor the deployment carefully

## 8. Creating New Migrations

```bash
# Navigate to database directory
cd database

# Create new migration
npx supabase migration new descriptive_name

# This creates: supabase/migrations/YYYYMMDDHHMMSS_descriptive_name.sql

# Edit the migration file
code supabase/migrations/YYYYMMDDHHMMSS_descriptive_name.sql

# Test locally (if using local Supabase)
npx supabase db reset  # Applies all migrations from scratch

# Push to dev
npx supabase db push

# Commit and push to trigger GitHub Actions
git add .
git commit -m "Add migration: descriptive_name"
git push origin main
```

## 9. Migration Best Practices

### Writing Safe Migrations

1. **Always use transactions** (migrations are wrapped in transactions by default)
2. **Test locally first** using local Supabase
3. **Make migrations reversible** when possible
4. **Avoid breaking changes** in production:
   - Don't drop columns that are in use
   - Don't rename tables/columns without a transition period
   - Add new columns as nullable first
5. **Use migrations for schema only**, not for data manipulation (use separate data scripts)

### Example Migration Structure

```sql
-- Migration: Add user preferences
-- Created: 2025-01-19

BEGIN;

-- Add new table
CREATE TABLE IF NOT EXISTS user_preferences (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  preferences JSONB DEFAULT '{}',
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Add indexes
CREATE INDEX idx_user_preferences_user_id ON user_preferences(user_id);

-- Enable RLS
ALTER TABLE user_preferences ENABLE ROW LEVEL SECURITY;

-- Create RLS policies
CREATE POLICY "Users can view their own preferences"
  ON user_preferences FOR SELECT
  USING (auth.uid() = user_id);

CREATE POLICY "Users can update their own preferences"
  ON user_preferences FOR UPDATE
  USING (auth.uid() = user_id);

COMMIT;
```

## 10. Troubleshooting

### Migration Failed

If a migration fails:

1. **Check the error message** in GitHub Actions logs
2. **Fix the migration file** locally
3. **Test the fix**:
   ```bash
   npx supabase db reset  # Resets and reapplies all migrations
   ```
4. **Commit and push** the fix

### Rollback a Migration

Supabase doesn't support automatic rollbacks. To rollback:

1. **Create a new migration** that reverses the changes:
   ```bash
   npx supabase migration new rollback_previous_change
   ```

2. **Write the rollback SQL**:
   ```sql
   -- Rollback: Remove user_preferences table
   DROP TABLE IF EXISTS user_preferences CASCADE;
   ```

3. **Apply the rollback** via GitHub Actions or manually

### Check Migration Status

```bash
# List applied migrations
npx supabase migration list

# Check database schema
npx supabase db diff

# Compare local vs remote
npx supabase db pull
```

### Connection Issues

If GitHub Actions can't connect:

1. **Verify project reference** is correct
2. **Check access token** is valid
3. **Verify database password** is correct
4. **Check Supabase project** is running (not paused)

## 11. Monitoring and Backups

### Enable Point-in-Time Recovery (Recommended for Production)

1. Go to Supabase Dashboard → Database → Backups
2. Enable "Point in Time Recovery" (PITR)
3. This allows you to restore to any point in the last 7 days

### Monitor Migration History

View migration history in Supabase:
```sql
SELECT * FROM supabase_migrations.schema_migrations
ORDER BY version DESC;
```

### Create Manual Backup Before Major Migrations

```bash
# Backup production database before major changes
npx supabase db dump -f backup-$(date +%Y%m%d).sql --db-url "postgresql://postgres:password@db.xxxxx.supabase.co:5432/postgres"
```

## 12. Environment Variables Summary

Make sure these are set correctly:

| Environment | Supabase URL | Supabase Anon Key | AWS Parameter Store |
|-------------|--------------|-------------------|---------------------|
| **Dev** | `https://xxxxx.supabase.co` | `eyJhbG...dev` | `/homely/dev/supabase_*` |
| **Prod** | `https://yyyyy.supabase.co` | `eyJhbG...prod` | `/homely/prod/supabase_*` |

## Support

For issues:
- Supabase Documentation: https://supabase.com/docs
- Supabase Discord: https://discord.supabase.com
- GitHub Actions Logs: Check workflow runs for detailed errors

---

**Last Updated**: 2025-01-19
**Supabase CLI Version**: Latest
