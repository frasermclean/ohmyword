targetScope = 'resourceGroup'

@description('The name of the App Configuration instance.')
param appConfigName string

@description('Cosmos DB database ID')
param cosmosDbDatabaseId string

@description('Azure AD audience')
param azureAdAudience string

@description('Azure AD client ID')
param azureAdClientId string

@description('Principal ID to assign access to')
param principalId string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@secure()
@description('RapidAPI key')
param rapidApiKey string

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
  name: 'kv-ohmyword-shared'

  resource rapidApiKeySecret 'secrets' = if (!empty(rapidApiKey)) {
    name: 'RapidApiKey-${appEnv}'
    properties: {
      value: rapidApiKey
      contentType: 'text/plain'
    }
  }
}

var appConfigurationKeyValues = [
  {
    name: 'AzureAd:ClientId$${appEnv}'
    value: azureAdClientId
    contentType: 'text/plain'
  }
  {
    name: 'AzureAd:Audience$${appEnv}'
    value: azureAdAudience
    contentType: 'text/plain'
  }
  {
    name: 'CosmosDb:DatabaseId$${appEnv}'
    value: cosmosDbDatabaseId
    contentType: 'text/plain'
  }
  {
    name: 'RapidApi:ApiKey$${appEnv}'
    value: '{"uri":"https://${keyVault.name}${environment().suffixes.keyvaultDns}/secrets/${keyVault::rapidApiKeySecret.name}"}'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
  }
]

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: appConfigName

  resource keyValue 'keyValues' = [for item in appConfigurationKeyValues: {
    name: item.name
    properties: {
      value: item.value
      contentType: item.contentType
    }
  }]
}

var appConfigurationRoleId = '516239f1-63e1-4d78-a4de-a74fb236a071' // App Configuration Data Reader
var keyVaultRoleId = '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User

// assign role to app configuration
resource appConfigurationRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, appConfigurationRoleId)
  scope: appConfiguration
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', appConfigurationRoleId)
    principalId: principalId
  }
}

// assign role to key vault
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, keyVaultRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', keyVaultRoleId)
    principalId: principalId
  }
}
