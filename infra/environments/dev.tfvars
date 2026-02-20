environment = "dev"
location    = "uksouth"

# PostgreSQL - burstable, small
postgresql_sku        = "B_Standard_B1ms"
postgresql_storage_mb = 32768

# Storage - locally redundant
storage_replication_type = "LRS"

# Container App - scale to zero
container_cpu          = 0.5
container_memory       = "1Gi"
container_min_replicas = 0
container_max_replicas = 2

# Logging
log_retention_days = 30

# Email - dry run in dev
email_dry_run      = true
email_app_base_url = "https://dev.be-lyke.clothing"
email_api_base_url = "https://dev-api.be-lyke.clothing"

# ASP.NET Core
aspnetcore_environment = "Development"
