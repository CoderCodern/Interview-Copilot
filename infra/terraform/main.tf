# Root Terraform config (Doc 08 §6). Remote state in S3 + DynamoDB lock, one per environment.
terraform {
  required_version = ">= 1.9"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.70"
    }
  }

  backend "s3" {
    bucket         = "interview-copilot-tfstate"
    key            = "env/terraform.tfstate" # workspace/env-scoped
    region         = "us-east-1"
    dynamodb_table = "interview-copilot-tflock"
    encrypt        = true
  }
}

provider "aws" {
  region = var.region
  default_tags {
    tags = {
      Project     = "interview-copilot"
      Environment = var.environment
      ManagedBy   = "terraform"
    }
  }
}

locals {
  name = "icp-${var.environment}"
}
