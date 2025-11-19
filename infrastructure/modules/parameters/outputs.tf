# Parameters Module Outputs

output "parameter_paths" {
  description = "Map of parameter store paths"
  value = {
    supabase_url      = aws_ssm_parameter.supabase_url.name
    supabase_anon_key = aws_ssm_parameter.supabase_anon_key.name
    backend_log_level = aws_ssm_parameter.backend_log_level.name
    cors_origins      = aws_ssm_parameter.cors_origins.name
  }
}

output "parameter_arns" {
  description = "Map of parameter store ARNs"
  value = {
    supabase_url      = aws_ssm_parameter.supabase_url.arn
    supabase_anon_key = aws_ssm_parameter.supabase_anon_key.arn
    backend_log_level = aws_ssm_parameter.backend_log_level.arn
    cors_origins      = aws_ssm_parameter.cors_origins.arn
  }
}
