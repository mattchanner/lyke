resource "azurerm_service_plan" "functions" {
  name                = "asp-lyke-${var.environment}-func"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = var.functions_sku

  tags = local.common_tags
}

resource "azurerm_linux_function_app" "media" {
  name                       = "func-lyke-${var.environment}-media"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  service_plan_id            = azurerm_service_plan.functions.id
  storage_account_name       = azurerm_storage_account.main.name
  storage_account_access_key = azurerm_storage_account.main.primary_access_key
  https_only                 = true

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME             = "dotnet-isolated"
    AzureStorage                         = local.storage_connection_string
    ConnectionStrings__DefaultConnection = local.postgresql_connection_string
  }

  site_config {
    # dotnet-isolated: the .NET runtime is embedded in the deployment artifact,
    # so the host version is informational only. Use 8.0 (latest LTS accepted by
    # the provider); the actual runtime is .NET 10 from the published binary.
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  tags = local.common_tags
}
