environment = "prod"
location    = "uksouth"

# PostgreSQL - general purpose, production
postgresql_sku        = "GP_Standard_D2s_v3"
postgresql_storage_mb = 65536

# Storage - geo-redundant
storage_replication_type = "GRS"

# Container App - always on
container_cpu          = 1.0
container_memory       = "2Gi"
container_min_replicas = 1
container_max_replicas = 5

# Logging
log_retention_days = 90

# Email - live in prod
email_dry_run      = false
email_app_base_url = "https://app.be-lyke.clothing"
email_api_base_url = "https://api.be-lyke.clothing"

# Functions - Elastic Premium (no cold start, VNet-capable)
functions_sku = "EP1"

# ASP.NET Core
aspnetcore_environment = "Production"
