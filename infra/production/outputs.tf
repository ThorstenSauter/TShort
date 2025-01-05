output "connection_string" {
  description = "The connection string used to connect to the Azure SQL database."
  value       = local.connection_string
}

output "deployment_token" {
  description = "The deployment token used to deploy code from CI pipelines."
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the SQL Server."
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "sql_server_database" {
  description = "The name of the SQL Server database."
  value       = azapi_resource.sql_database.name
}
