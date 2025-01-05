locals {
  connection_string                  = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.main.name};Authentication=Active Directory Default;User ID=${azurerm_user_assigned_identity.api.client_id};azurerm_user_assigned_identity.api.client_id};Encrypt=True;Connection Timeout=30;"
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
    data.azuread_client_config.current.object_id
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

resource "azurerm_mssql_firewall_rule" "main" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "main" {
  name        = local.database_name
  server_id   = azurerm_mssql_server.main.id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  sku_name    = "Basic"
  max_size_gb = 1
  tags        = var.tags
  lifecycle {
    prevent_destroy = true
  }
}

resource "mssql_user" "api_identity" {
  server {
    host = azurerm_mssql_server.main.fully_qualified_domain_name
    azuread_default_chain_auth {
    }
  }

  database  = azurerm_mssql_database.main.name
  username  = azurerm_user_assigned_identity.api.name
  object_id = azurerm_user_assigned_identity.api.client_id

  roles = ["db_datareader", "db_datawriter"]
}
