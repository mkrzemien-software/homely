# ACM Module Outputs

output "certificate_arn" {
  description = "ARN of the ACM certificate"
  value       = aws_acm_certificate.main.arn
}

output "certificate_id" {
  description = "ID of the ACM certificate"
  value       = aws_acm_certificate.main.id
}

output "certificate_status" {
  description = "Status of the ACM certificate"
  value       = aws_acm_certificate.main.status
}

output "domain_validation_options" {
  description = "Domain validation options (for manual DNS configuration)"
  value       = aws_acm_certificate.main.domain_validation_options
  sensitive   = false
}

output "validation_records" {
  description = "DNS validation records to add to your DNS provider"
  value = [
    for dvo in aws_acm_certificate.main.domain_validation_options : {
      name   = dvo.resource_record_name
      type   = dvo.resource_record_type
      value  = dvo.resource_record_value
      domain = dvo.domain_name
    }
  ]
}

