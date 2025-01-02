locals {
  web_redirect_uris = [
    "https://admin.tshort.me/authentication/login-callback",
    "https://staging.admin.tshort.me/authentication/login-callback",
    "http://localhost:5010/authentication/login-callback",
    "https://localhost:7045/authentication/login-callback"
  ]
}

resource "azuread_application_registration" "api" {
  display_name     = "TShort API"
  description      = "Used to authenticate to the TShort API"
  sign_in_audience = "AzureADMyOrg"
  homepage_url     = var.api_uri
}

resource "azuread_service_principal" "api" {
  client_id = azuread_application_registration.api.client_id
}

resource "azuread_application_identifier_uri" "api" {
  application_id = azuread_application_registration.api.id
  identifier_uri = "api://${azuread_application_registration.api.client_id}"
}

resource "azuread_application_api_access" "api" {
  application_id = azuread_application_registration.api.id
  api_client_id  = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

  scope_ids = [
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"],
  ]
}

resource "random_uuid" "api_access_as_user" {}

resource "azuread_application_permission_scope" "api_access_as_user" {
  application_id = azuread_application_registration.api.id
  scope_id       = random_uuid.api_access_as_user.id
  value          = "access_as_user"

  admin_consent_description  = "This allows a user to access the API."
  admin_consent_display_name = "Access API as user"

  user_consent_description  = "This allows you to access the API."
  user_consent_display_name = "Access API as user"
}

resource "azuread_application_registration" "web" {
  display_name     = "TShort Webinterface"
  description      = "Used to administer the TShort service."
  sign_in_audience = "AzureADMyOrg"
  homepage_url     = var.web_uri
}

resource "azuread_service_principal" "web" {
  client_id = azuread_application_registration.web.client_id
}

resource "azuread_application_redirect_uris" "web" {
  application_id = azuread_application_registration.web.id
  redirect_uris  = local.web_redirect_uris
  type           = "SPA"
}

resource "azuread_application_api_access" "web_graph" {
  application_id = azuread_application_registration.web.id
  api_client_id  = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

  scope_ids = [
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["email"],
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["offline_access"],
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["openid"],
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["profile"],
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"],
  ]
}

resource "azuread_application_api_access" "web_api" {
  application_id = azuread_application_registration.web.id
  api_client_id  = azuread_application_registration.api.client_id

  scope_ids = [
    azuread_application_permission_scope.api_access_as_user.scope_id
  ]
}
