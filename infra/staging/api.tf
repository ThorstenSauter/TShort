locals {
  api_container_app_name         = "aca-${local.app_name}-api-${var.env}-${var.resource_id}"
  api_custom_domain_is_apex      = var.api_custom_domain == "@"
  application_insights_name      = "appi-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
  container_app_environment_name = "cae-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
  container_registry_name        = split(".", local.container_registry_server)[0]
  container_registry_server      = split("/", var.api_container_image_name)[0]
  full_api_custom_domain = (
    local.api_custom_domain_is_apex
    ? var.dns_zone
    : "${var.api_custom_domain}.${var.dns_zone}"
  )
  log_analytics_workspace_name = "log-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
  user_assigned_identity_name  = "id-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
}

data "azurerm_container_registry" "main" {
  name                = local.container_registry_name
  resource_group_name = var.container_registry_resource_group
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = local.log_analytics_workspace_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_application_insights" "main" {
  name                = local.application_insights_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  application_type    = "web"
  tags                = var.tags
}

resource "azurerm_user_assigned_identity" "api" {
  name                = local.user_assigned_identity_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tags                = var.tags
}

resource "azurerm_role_assignment" "api_acr_pull" {
  principal_id         = azurerm_user_assigned_identity.api.principal_id
  scope                = data.azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
}

resource "azurerm_container_app_environment" "main" {
  name                       = local.container_app_environment_name
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }
  tags = var.tags
}

resource "azurerm_container_app" "api" {
  name                         = local.api_container_app_name
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  identity {
    type = "UserAssigned"
    identity_ids = [
      azurerm_user_assigned_identity.api.id
    ]
  }
  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
  registry {
    server   = data.azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.api.id
  }
  template {
    container {
      name   = "${local.app_name}-api"
      image  = var.api_container_image_name
      cpu    = 0.25
      memory = "0.5Gi"
      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = azurerm_application_insights.main.connection_string
      }
    }
  }
  tags = var.tags
  depends_on = [
    azurerm_role_assignment.api_acr_pull
  ]
}

resource "cloudflare_record" "asuid_api" {
  zone_id = data.cloudflare_zone.main.zone_id
  name    = local.api_custom_domain_is_apex ? "asuid" : "asuid.${var.api_custom_domain}"
  type    = "TXT"
  content = azurerm_container_app.api.custom_domain_verification_id
  comment = "Azure custom domain verification id"
}

resource "cloudflare_record" "api" {
  zone_id = data.cloudflare_zone.main.zone_id
  name    = local.api_custom_domain_is_apex ? var.dns_zone : var.api_custom_domain
  type    = local.api_custom_domain_is_apex ? "A" : "CNAME"
  content = (
    local.api_custom_domain_is_apex
    ? azurerm_container_app_environment.main.static_ip_address
    : "${azurerm_container_app.api.name}.${azurerm_container_app_environment.main.default_domain}"
  )
  comment = "Custom domain for the ${local.app_name} API ${var.env} environment"
}

resource "time_sleep" "api_custom_domain_records" {
  create_duration = "2m"
  depends_on = [
    cloudflare_record.asuid_api,
    cloudflare_record.api
  ]
}

resource "azurerm_container_app_custom_domain" "api" {
  name             = local.full_api_custom_domain
  container_app_id = azurerm_container_app.api.id
  depends_on = [
    time_sleep.api_custom_domain_records
  ]
  lifecycle {
    ignore_changes = [
      certificate_binding_type, container_app_environment_certificate_id
    ]
  }
}
