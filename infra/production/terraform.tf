terraform {
  required_version = ">= 1.10.0"
  required_providers {
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.3.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.2.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.24.0"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 5.2.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.13.0"
    }
  }
}
