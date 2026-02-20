resource "azurerm_communication_service" "main" {
  name                = "acs-lyke-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  data_location       = "Europe"

  tags = local.common_tags
}

locals {
  email_connection_string = azurerm_communication_service.main.primary_connection_string
}
