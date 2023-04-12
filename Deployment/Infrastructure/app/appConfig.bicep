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

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
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
    name: 'WordsApi:ApiKey$${appEnv}'
    value: '{"uri":"https://${keyVault.name}.${environment().suffixes.keyvaultDns}/secrets/${keyVault::rapidApiKeySecret.name}"}'
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

resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '516239f1-63e1-4d78-a4de-a74fb236a071' // app configuration data reader
  scope: resourceGroup()
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, roleDefinition.id, principalId)
  scope: appConfiguration
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
