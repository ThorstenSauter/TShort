provider "azapi" {}

provider "azurerm" {
  storage_use_azuread = true
  features {
    resource_group {
      prevent_deletion_if_contains_resources = true
    }
  }
}

provider "cloudflare" {}

provider "time" {}
