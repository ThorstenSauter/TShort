variable "env" {
  type        = string
  description = "The shorthand name of the environment the infrastructure is deployed to."
}

variable "location" {
  type        = string
  description = "The Azure region where resources are deployed."
}

variable "primary_domain" {
  type        = string
  description = "The primary domain for the static web app."
}

variable "resource_id" {
  type        = string
  description = "The id appended to resource names in order to make them unique."
}

variable "tags" {
  type = map(string)
  description = "The default tags for Azure resources."
}
