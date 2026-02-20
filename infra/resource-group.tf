resource "azurerm_resource_group" "main" {
  name     = "rg-lyke-${var.environment}"
  location = var.location

  tags = local.common_tags
}

locals {
  common_tags = {
    project     = "lyke"
    environment = var.environment
    managed_by  = "terraform"
  }
}
