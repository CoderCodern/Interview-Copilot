# PostgreSQL 16 + pgvector on RDS, Multi-AZ in prod (Doc 04, Doc 08 §2).
resource "aws_db_subnet_group" "main" {
  name       = "${local.name}-db"
  subnet_ids = aws_subnet.private[*].id
}

resource "aws_db_parameter_group" "pg16" {
  name   = "${local.name}-pg16"
  family = "postgres16"
  # pgvector is created via migration; shared_preload not required for the extension.
}

resource "aws_db_instance" "postgres" {
  identifier                 = "${local.name}-postgres"
  engine                     = "postgres"
  engine_version             = "16"
  instance_class             = var.db_instance_class
  allocated_storage          = 50
  max_allocated_storage      = 500
  storage_encrypted          = true
  multi_az                   = var.db_multi_az
  db_subnet_group_name       = aws_db_subnet_group.main.name
  vpc_security_group_ids     = [aws_security_group.rds.id]
  parameter_group_name       = aws_db_parameter_group.pg16.name
  backup_retention_period    = var.environment == "prod" ? 7 : 1
  deletion_protection        = var.environment == "prod"
  auto_minor_version_upgrade = true
  skip_final_snapshot        = var.environment != "prod"
  db_name                    = "interviewcopilot"
  username                   = "app"
  manage_master_user_password = true # rotated in Secrets Manager (Doc 10 §5)
}
