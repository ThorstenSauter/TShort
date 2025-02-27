variable "api_container_image_name" {
  type        = string
  description = "The name of the API container image to deploy."
}

variable "api_custom_domain" {
  type        = string
  description = "The custom domain for the API. Should only contain the subdomain or an '@' for the apex domain."
}

variable "container_registry_resource_group" {
  type        = string
  description = "The name of the resource group where the container registry is deployed."
}

variable "dns_zone" {
  type        = string
  description = "The DNS zone for the custom domains."
}

variable "entra_id_client_secret" {
  type        = string
  description = "The client secret for the Entra ID application."
  sensitive   = true
}

variable "env" {
  type        = string
  description = "The shorthand name of the environment the infrastructure is deployed to."
}

variable "location" {
  type        = string
  description = "The Azure region where resources are deployed."
}

variable "primary_web_custom_domain" {
  type        = string
  description = "The custom domain for the webinterface. Should only contain the subdomain or an '@' for the apex domain."
}

variable "resource_id" {
  type        = string
  description = "The id appended to resource names in order to make them unique."
}

variable "tags" {
  type        = map(string)
  description = "The default tags for Azure resources."
}
