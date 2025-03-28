terraform {
  required_version = ">= 1.10.0"
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.2.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.25.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.13.0"
    }
  }
}
