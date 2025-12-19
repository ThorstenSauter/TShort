terraform {
  required_version = ">= 1.10.0"
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.7.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.57.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.13.0"
    }
  }
}
