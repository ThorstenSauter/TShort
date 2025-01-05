locals {
  connection_string                  = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=${azapi_resource.sql_database.name};Authentication=Active Directory Default;User ID=${azurerm_user_assigned_identity.api.client_id};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  database_administrators_group_name = "SQL-Admins-${local.database_server_name}"
  database_name                      = "sqldb-${local.app_name}"
  database_server_name               = "sql-${local.app_name}-${var.env}-${var.location}-${var.resource_id}"
}

resource "azuread_group" "sql_server_admins" {
  display_name     = local.database_administrators_group_name
  description      = "SQL Server Admins for Server ${local.database_server_name}"
  owners           = [data.azuread_client_config.current.object_id]
  security_enabled = true
  members = [
    data.azuread_client_config.current.object_id,
    azurerm_user_assigned_identity.api.principal_id
  ]
}

resource "time_sleep" "group_provisioning" {
  create_duration = "30s"
  triggers = {
    display_name = azuread_group.sql_server_admins.display_name
    object_id    = azuread_group.sql_server_admins.object_id
  }
}

#trivy:ignore:avd-azu-0022
#trivy:ignore:avd-azu-0027
resource "azurerm_mssql_server" "main" {
  name                = local.database_server_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  version             = "12.0"
  minimum_tls_version = "1.2"
  azuread_administrator {
    login_username              = time_sleep.group_provisioning.triggers["display_name"]
    object_id                   = time_sleep.group_provisioning.triggers["object_id"]
    azuread_authentication_only = true
  }
  tags = var.tags
}

resource "azapi_resource" "sql_database" {
  type      = "Microsoft.Sql/servers/databases@2023-08-01-preview"
  name      = "sqldb-001"
  location  = azurerm_resource_group.main.location
  parent_id = azurerm_mssql_server.main.id

  body = {
    properties = {
      minCapacity                      = 0.5
      maxSizeBytes                     = 34359738368
      autoPauseDelay                   = 15
      zoneRedundant                    = false
      isLedgerOn                       = false
      useFreeLimit                     = true
      readScale                        = "Disabled"
      freeLimitExhaustionBehavior      = "BillOverUsage"
      availabilityZone                 = "NoPreference"
      requestedBackupStorageRedundancy = "Local"
    }

    sku = {
      name     = "GP_S_Gen5"
      tier     = "GeneralPurpose"
      family   = "Gen5"
      capacity = 2
    }
  }

  schema_validation_enabled = false
  response_export_values    = ["*"]
}
