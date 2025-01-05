terraform {
  required_version = ">= 1.10.0"
  required_providers {
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.2.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.0.2"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.14.0"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 4.49.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "0.12.1"
    }
  }
}
