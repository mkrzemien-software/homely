# Parameters Module Outputs

output "parameter_paths" {
  description = "Map of parameter store paths"
  value = {
    supabase_url                 = aws_ssm_parameter.supabase_url.name
    supabase_anon_key            = aws_ssm_parameter.supabase_anon_key.name
    supabase_service_role_key    = aws_ssm_parameter.supabase_service_role_key.name
    database_connection_string   = aws_ssm_parameter.database_connection_string.name
    backend_log_level            = aws_ssm_parameter.backend_log_level.name
    cors_origins                 = aws_ssm_parameter.cors_origins.name
    jwt_valid_issuer             = aws_ssm_parameter.jwt_valid_issuer.name
    jwt_valid_audience           = aws_ssm_parameter.jwt_valid_audience.name
    jwt_secret                   = aws_ssm_parameter.jwt_secret.name
  }
}

output "parameter_arns" {
  description = "Map of parameter store ARNs"
  value = {
    supabase_url                 = aws_ssm_parameter.supabase_url.arn
    supabase_anon_key            = aws_ssm_parameter.supabase_anon_key.arn
    supabase_service_role_key    = aws_ssm_parameter.supabase_service_role_key.arn
    database_connection_string   = aws_ssm_parameter.database_connection_string.arn
    backend_log_level            = aws_ssm_parameter.backend_log_level.arn
    cors_origins                 = aws_ssm_parameter.cors_origins.arn
    jwt_valid_issuer             = aws_ssm_parameter.jwt_valid_issuer.arn
    jwt_valid_audience           = aws_ssm_parameter.jwt_valid_audience.arn
    jwt_secret                   = aws_ssm_parameter.jwt_secret.arn
  }
}
