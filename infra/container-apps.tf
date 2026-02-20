resource "azurerm_container_app_environment" "main" {
  name                       = "cae-lyke-${var.environment}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id

  tags = local.common_tags
}

resource "azurerm_container_app" "api" {
  name                         = "ca-lyke-${var.environment}-api"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  tags = local.common_tags

  # --- Secrets ---
  secret {
    name  = "db-connection-string"
    value = local.postgresql_connection_string
  }

  secret {
    name  = "storage-connection-string"
    value = local.storage_connection_string
  }

  secret {
    name  = "email-connection-string"
    value = local.email_connection_string
  }

  secret {
    name  = "jwt-secret"
    value = var.jwt_secret
  }

  secret {
    name  = "google-client-id"
    value = var.google_client_id
  }

  secret {
    name  = "apple-app-id"
    value = var.apple_app_id
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.main.admin_password
  }

  # --- Registry ---
  registry {
    server               = azurerm_container_registry.main.login_server
    username             = azurerm_container_registry.main.admin_username
    password_secret_name = "acr-password"
  }

  # --- Ingress ---
  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  # --- Template ---
  template {
    min_replicas = var.container_min_replicas
    max_replicas = var.container_max_replicas

    container {
      name   = "lyke-api"
      image  = "${azurerm_container_registry.main.login_server}/lyke-api:latest"
      cpu    = var.container_cpu
      memory = var.container_memory

      # --- Connection Strings ---
      env {
        name        = "ConnectionStrings__DefaultConnection"
        secret_name = "db-connection-string"
      }

      # --- Azure Blob Storage ---
      env {
        name        = "AzureBlob__ConnectionString"
        secret_name = "storage-connection-string"
      }
      env {
        name  = "AzureBlob__ContainerName"
        value = "media"
      }

      # --- Email ---
      env {
        name        = "Email__ConnectionString"
        secret_name = "email-connection-string"
      }
      env {
        name  = "Email__SenderAddress"
        value = var.email_sender_address
      }
      env {
        name  = "Email__SenderDisplayName"
        value = "Do not reply"
      }
      env {
        name  = "Email__AppBaseUrl"
        value = var.email_app_base_url
      }
      env {
        name  = "Email__ApiBaseUrl"
        value = var.email_api_base_url
      }
      env {
        name  = "Email__DryRun"
        value = tostring(var.email_dry_run)
      }

      # --- JWT ---
      env {
        name        = "Jwt__Secret"
        secret_name = "jwt-secret"
      }
      env {
        name  = "Jwt__Issuer"
        value = "Lyke.Api"
      }
      env {
        name  = "Jwt__Audience"
        value = "Lyke.App"
      }

      # --- Social Auth ---
      env {
        name        = "SocialAuth__GoogleClientId"
        secret_name = "google-client-id"
      }
      env {
        name        = "SocialAuth__AppleAppId"
        secret_name = "apple-app-id"
      }

      # --- ASP.NET Core ---
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.aspnetcore_environment
      }

      # --- Health Probes ---
      startup_probe {
        transport = "HTTP"
        path      = "/health"
        port      = 8080
        # Generous timeout for EF Core migrations on cold start
        initial_delay           = 10
        interval_seconds        = 5
        timeout                 = 3
        failure_count_threshold = 30
      }

      liveness_probe {
        transport               = "HTTP"
        path                    = "/health"
        port                    = 8080
        interval_seconds        = 30
        timeout                 = 3
        failure_count_threshold = 3
      }

      readiness_probe {
        transport               = "HTTP"
        path                    = "/health"
        port                    = 8080
        interval_seconds        = 10
        timeout                 = 3
        failure_count_threshold = 3
      }
    }
  }
}
