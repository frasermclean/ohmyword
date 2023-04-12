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

var keyValues = [
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
]

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: appConfigName

  resource keyValue 'keyValues' = [for item in keyValues: {
    name: item.name
    properties: {
      value: item.value
      contentType: item.contentType
    }
  }]
}
