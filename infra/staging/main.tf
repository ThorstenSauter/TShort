data "azuread_client_config" "current" {}

data "azurerm_client_config" "current" {}

data "cloudflare_zone" "main" {
  filter = {
    name = var.dns_zone
  }
}

locals {
  resource_group_name = "rg-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
}

resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.location
  tags     = var.tags
}
