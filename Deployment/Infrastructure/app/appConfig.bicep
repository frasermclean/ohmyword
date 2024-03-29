targetScope = 'resourceGroup'

@description('The name of the App Configuration instance.')
param appConfigName string

@description('Cosmos DB database ID')
param cosmosDbDatabaseId string

@description('Azure AD audience')
param azureAdAudience string

@description('Azure AD client ID')
param azureAdClientId string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('IP lookup feature enabled')
param ipLookupFeatureEnabled bool = true

@description('Azure SignalR Service hostname')
param signalRServiceHostname string

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
  name: 'ohmyword-kv'

  resource rapidApiKeySecret 'secrets' existing = {
    name: 'rapidApi-key-${appEnv}'
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
  {
    name: 'ServiceBus:IpLookupQueueName$${appEnv}'
    value: 'shared-ip-lookup'
    contentType: 'text/plain'
  }
  {
    name: 'SignalRService:ConnectionString$${appEnv}'
    value: 'Endpoint=https://${signalRServiceHostname};AuthType=azure.msi;Version=1.0;'
    contentType: 'text/plain'
  }
]

var featureFlags = [
  {
    key: 'IpLookup'
    value: {
      id: 'IpLookup'
      description: 'Send IP lookup requests to a queue for processing'
      enabled: ipLookupFeatureEnabled
    }
  }
]

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigName

  resource keyValue 'keyValues' = [for item in appConfigurationKeyValues: {
    name: item.name
    properties: {
      value: item.value
      contentType: item.contentType
    }
  }]

  resource featureFlag 'keyValues' = [for flag in featureFlags: {
    name: '.appconfig.featureflag~2F${flag.key}$${appEnv}'
    properties: {
      value: string(flag.value)
      contentType: 'application/vnd.microsoft.appconfig.ff+json;charset=utf-8'
    }
  }]
}
