output "registry_name" {
  value       = azurerm_container_registry.main.name
  description = "The name of the Azure container registry"
}

output "resource_group_name" {
  value       = azurerm_resource_group.main.name
  description = "The name of the Azure resource group"
}
