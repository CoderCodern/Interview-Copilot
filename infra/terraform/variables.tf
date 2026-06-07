variable "environment" {
  description = "Deployment environment (dev|staging|prod)"
  type        = string
}

variable "region" {
  type    = string
  default = "us-east-1"
}

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}

variable "az_count" {
  description = "Number of AZs (2 for prod HA — Doc 08 §3)"
  type        = number
  default     = 2
}

variable "api_image" {
  description = "ECR image URI (tag = git SHA, set by CD — Doc 09 §3)"
  type        = string
}

variable "web_image" {
  type = string
}

variable "db_instance_class" {
  type    = string
  default = "db.r6g.large"
}

variable "db_multi_az" {
  type    = bool
  default = true
}

variable "domain_name" {
  type    = string
  default = "app.interviewcopilot.ai"
}

variable "otel_endpoint" {
  description = "OTLP collector endpoint for traces/metrics/logs (Doc 11)"
  type        = string
  default     = "http://otel-collector:4317"
}
