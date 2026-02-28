output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "container_app_url" {
  value = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}

output "container_registry_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "postgresql_fqdn" {
  value = azurerm_postgresql_flexible_server.main.fqdn
}

output "storage_account_name" {
  value = azurerm_storage_account.main.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "function_app_url" {
  value = "https://${azurerm_linux_function_app.media.default_hostname}"
}
