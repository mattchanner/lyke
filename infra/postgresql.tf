resource "azurerm_postgresql_flexible_server" "main" {
  name                          = "psql-lyke-${var.environment}"
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = "16"
  administrator_login           = var.postgresql_admin_username
  administrator_password        = var.postgresql_admin_password
  sku_name                      = var.postgresql_sku
  storage_mb                    = var.postgresql_storage_mb
  backup_retention_days         = 7
  geo_redundant_backup_enabled  = var.environment == "prod"
  public_network_access_enabled = true
  zone                          = "1"

  tags = local.common_tags
}

resource "azurerm_postgresql_flexible_server_database" "lyke" {
  name      = "lyke"
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Allow Azure services to connect
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

locals {
  postgresql_connection_string = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=lyke;Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;Trust Server Certificate=true"
}
