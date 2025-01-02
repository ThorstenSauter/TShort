data "azurerm_client_config" "current" {}

data "cloudflare_zone" "main" {
  name = local.root_domain
}

locals {
  app_name            = "tshort"
  domain_splits       = split(".", var.primary_domain)
  domain_split_length = length(local.domain_splits)
  tld                 = local.domain_splits[local.domain_split_length - 1]
  root_domain         = join(".", [local.domain_splits[local.domain_split_length - 2], local.tld])
  subdomain           = coalesce(join(".", slice(local.domain_splits, 0, local.domain_split_length - 2)), var.primary_domain)
  resource_group_name = "rg-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
  static_site_name    = "stapp-${local.app_name}-${var.env}-${var.resource_id}"
}

resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_static_web_app" "main" {
  name                = local.static_site_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku_size            = "Free"
  sku_tier            = "Free"
  tags                = var.tags
}

resource "cloudflare_record" "primary" {
  zone_id = data.cloudflare_zone.main.id
  name    = local.subdomain
  type    = "CNAME"
  content = azurerm_static_web_app.main.default_host_name
  comment = "Primary domain for TShort ${var.env} Azure Static Web App"
}

resource "time_sleep" "wait_for_dns_record" {
  depends_on      = [cloudflare_record.primary]
  create_duration = "2m"
}

resource "azurerm_static_web_app_custom_domain" "primary" {
  static_web_app_id = azurerm_static_web_app.main.id
  domain_name       = var.primary_domain
  validation_type   = "cname-delegation"
  depends_on        = [time_sleep.wait_for_dns_record]
}
