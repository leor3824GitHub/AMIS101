terraform {
  required_version = ">= 1.14.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.80.0"
    }
  }
}

provider "aws" {
  region = var.region

  default_tags {
    tags = {
      Environment = var.environment
      Project     = "dotnet-starter-kit"
      ManagedBy   = "terraform"
    }
  }
}

module "app" {
  source = "../../../app_stack"

  environment = var.environment
  region      = var.region
  domain_name = var.domain_name

  # Network
  vpc_cidr_block  = var.vpc_cidr_block
  public_subnets  = var.public_subnets
  private_subnets = var.private_subnets

  enable_nat_gateway = var.enable_nat_gateway
  single_nat_gateway = var.single_nat_gateway

  enable_s3_endpoint             = var.enable_s3_endpoint
  enable_ecr_endpoints           = var.enable_ecr_endpoints
  enable_logs_endpoint           = var.enable_logs_endpoint
  enable_secretsmanager_endpoint = var.enable_secretsmanager_endpoint
  enable_flow_logs               = var.enable_flow_logs

  # ECS
  enable_container_insights = var.enable_container_insights

  # ALB
  enable_https        = var.enable_https
  acm_certificate_arn = var.acm_certificate_arn

  # S3
  app_s3_bucket_name            = var.app_s3_bucket_name
  app_s3_enable_public_read     = var.app_s3_enable_public_read
  app_s3_enable_cloudfront      = var.app_s3_enable_cloudfront
  app_s3_cloudfront_price_class = var.app_s3_cloudfront_price_class

  # Database
  db_name                        = var.db_name
  db_username                    = var.db_username
  db_password                    = var.db_password
  db_manage_master_user_password = var.db_manage_master_user_password
  db_instance_class              = var.db_instance_class
  db_engine_version              = var.db_engine_version
  db_multi_az                    = var.db_multi_az
  db_deletion_protection         = var.db_deletion_protection
  db_enable_performance_insights = var.db_enable_performance_insights

  # Redis
  redis_node_type                  = var.redis_node_type
  redis_num_cache_clusters         = var.redis_num_cache_clusters
  redis_automatic_failover_enabled = var.redis_automatic_failover_enabled

  # Container Images
  container_registry  = var.container_registry
  container_image_tag = var.container_image_tag
  api_image_name      = var.api_image_name
  blazor_image_name   = var.blazor_image_name

  # API Service
  api_container_port         = var.api_container_port
  api_cpu                    = var.api_cpu
  api_memory                 = var.api_memory
  api_desired_count          = var.api_desired_count
  api_enable_circuit_breaker = var.api_enable_circuit_breaker
  api_use_fargate_spot       = var.api_use_fargate_spot

  # Blazor Service
  blazor_container_port         = var.blazor_container_port
  blazor_cpu                    = var.blazor_cpu
  blazor_memory                 = var.blazor_memory
  blazor_desired_count          = var.blazor_desired_count
  blazor_enable_circuit_breaker = var.blazor_enable_circuit_breaker
  blazor_use_fargate_spot       = var.blazor_use_fargate_spot
}

################################################################################
# Outputs
################################################################################

output "vpc_id" {
  description = "VPC ID."
  value       = module.app.vpc_id
}

output "alb_dns_name" {
  description = "ALB DNS name."
  value       = module.app.alb_dns_name
}

output "api_url" {
  description = "API URL."
  value       = module.app.api_url
}

output "blazor_url" {
  description = "Blazor URL."
  value       = module.app.blazor_url
}

output "rds_endpoint" {
  description = "RDS endpoint."
  value       = module.app.rds_endpoint
}

output "rds_secret_arn" {
  description = "RDS secret ARN (if using managed password)."
  value       = module.app.rds_secret_arn
}

output "redis_endpoint" {
  description = "Redis endpoint."
  value       = module.app.redis_endpoint
}

output "s3_bucket_name" {
  description = "S3 bucket name."
  value       = module.app.s3_bucket_name
}

output "s3_cloudfront_domain" {
  description = "CloudFront domain."
  value       = module.app.s3_cloudfront_domain != "" ? "https://${module.app.s3_cloudfront_domain}" : ""
}
