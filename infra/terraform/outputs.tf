output "vpc_id" {
  value = aws_vpc.main.id
}

output "ecs_cluster" {
  value = aws_ecs_cluster.main.name
}

output "db_endpoint" {
  value     = aws_db_instance.postgres.address
  sensitive = true
}

output "alb_dns_name" {
  description = "Point the app DNS/CloudFront origin at this (Doc 08 §1)."
  value       = aws_lb.main.dns_name
}
