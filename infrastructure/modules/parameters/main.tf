# Parameters Module
# Creates AWS Systems Manager Parameter Store parameters for application configuration

locals {
  name_prefix     = "${var.project_name}-${var.environment}"
  parameter_prefix = "/${var.project_name}/${var.environment}"
}

# Supabase URL (SecureString)
resource "aws_ssm_parameter" "supabase_url" {
  name        = "${local.parameter_prefix}/supabase_url"
  description = "Supabase project URL"
  type        = "SecureString"
  value       = nonsensitive(var.supabase_url) != "" ? var.supabase_url : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-supabase-url"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}

# Supabase Anonymous Key (SecureString)
resource "aws_ssm_parameter" "supabase_anon_key" {
  name        = "${local.parameter_prefix}/supabase_anon_key"
  description = "Supabase anonymous key"
  type        = "SecureString"
  value       = nonsensitive(var.supabase_anon_key) != "" ? var.supabase_anon_key : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-supabase-anon-key"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}

# Supabase Service Role Key (SecureString)
resource "aws_ssm_parameter" "supabase_service_role_key" {
  name        = "${local.parameter_prefix}/supabase_service_role_key"
  description = "Supabase service role key for admin operations"
  type        = "SecureString"
  value       = nonsensitive(var.supabase_service_role_key) != "" ? var.supabase_service_role_key : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-supabase-service-role-key"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}

# Database Connection String (SecureString)
resource "aws_ssm_parameter" "database_connection_string" {
  name        = "${local.parameter_prefix}/database_connection_string"
  description = "PostgreSQL database connection string"
  type        = "SecureString"
  value       = nonsensitive(var.database_connection_string) != "" ? var.database_connection_string : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-db-connection-string"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}

# Backend Log Level (String)
resource "aws_ssm_parameter" "backend_log_level" {
  name        = "${local.parameter_prefix}/backend_log_level"
  description = "Backend application log level"
  type        = "String"
  value       = var.backend_log_level

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-backend-log-level"
    }
  )
}

# CORS Origins (String)
resource "aws_ssm_parameter" "cors_origins" {
  name        = "${local.parameter_prefix}/cors_origins"
  description = "Allowed CORS origins (comma-separated)"
  type        = "String"
  value       = var.cors_origins

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-cors-origins"
    }
  )
}

# JWT Valid Issuer (SecureString)
resource "aws_ssm_parameter" "jwt_valid_issuer" {
  name        = "${local.parameter_prefix}/jwt_valid_issuer"
  description = "JWT token issuer URL"
  type        = "SecureString"
  value       = nonsensitive(var.jwt_valid_issuer) != "" ? var.jwt_valid_issuer : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-jwt-issuer"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}

# JWT Valid Audience (String)
resource "aws_ssm_parameter" "jwt_valid_audience" {
  name        = "${local.parameter_prefix}/jwt_valid_audience"
  description = "JWT token audience"
  type        = "String"
  value       = var.jwt_valid_audience

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-jwt-audience"
    }
  )
}

# JWT Secret (SecureString)
resource "aws_ssm_parameter" "jwt_secret" {
  name        = "${local.parameter_prefix}/jwt_secret"
  description = "JWT signing secret from Supabase"
  type        = "SecureString"
  value       = nonsensitive(var.jwt_secret) != "" ? var.jwt_secret : "PLACEHOLDER_SET_MANUALLY"

  tags = merge(
    var.tags,
    {
      Name = "${local.name_prefix}-jwt-secret"
    }
  )

  lifecycle {
    ignore_changes = [value] # Prevent Terraform from overwriting manually set values
  }
}