locals {
  container_registry_name = "cr${local.app_name}${var.location}${var.resource_id}"
  resource_group_name     = "rg-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
}

resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_container_registry" "main" {
  name                = local.container_registry_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = false
  tags                = var.tags
}

resource "azurerm_role_assignment" "acr_push" {
  scope                = azurerm_container_registry.main.id
  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = "AcrPush"
}
