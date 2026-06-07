# ECS Fargate cluster + services for api/web/worker with autoscaling (Doc 08 §2/§4).
resource "aws_ecs_cluster" "main" {
  name = "${local.name}-cluster"
  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}

resource "aws_ecs_task_definition" "api" {
  family                   = "${local.name}-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "512"
  memory                   = "1024"
  execution_role_arn       = aws_iam_role.task_execution.arn
  task_role_arn            = aws_iam_role.api_task.arn

  container_definitions = jsonencode([
    {
      name      = "api"
      image     = var.api_image
      essential = true
      portMappings = [{ containerPort = 8080 }]
      # Secrets injected from Secrets Manager at start (Doc 10 §5).
      secrets = [
        { name = "ConnectionStrings__Postgres", valueFrom = aws_secretsmanager_secret.db_conn.arn }
      ]
      environment = [
        { name = "ASPNETCORE_URLS", value = "http://+:8080" },
        { name = "OpenTelemetry__Endpoint", value = var.otel_endpoint }
      ]
      healthCheck = {
        command  = ["CMD-SHELL", "curl -f http://localhost:8080/health/live || exit 1"]
        interval = 30
        timeout  = 5
        retries  = 3
      }
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/${local.name}-api"
          "awslogs-region"        = var.region
          "awslogs-stream-prefix" = "api"
        }
      }
    }
  ])
}

resource "aws_ecs_service" "api" {
  name            = "${local.name}-api"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count   = var.environment == "prod" ? 2 : 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets         = aws_subnet.private[*].id
    security_groups = [aws_security_group.api.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "api"
    container_port   = 8080
  }

  # Auto-rollback on failed deploys (Doc 09 §3).
  deployment_circuit_breaker {
    enable   = true
    rollback = true
  }
}

# Worker service scales on SQS queue depth; web service mirrors api (Doc 08 §4).
# autoscaling.tf defines target-tracking policies (CPU + ALB requests + queue depth).
