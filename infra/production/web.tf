locals {
  static_site_name           = "stapp-${local.app_name}-${var.env}-${var.resource_id}"
  full_primary_custom_domain = "${var.primary_web_custom_domain}.${var.dns_zone}"
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
  name    = var.primary_web_custom_domain
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
  domain_name       = local.full_primary_custom_domain
  validation_type   = "cname-delegation"
  depends_on        = [time_sleep.wait_for_dns_record]
}
