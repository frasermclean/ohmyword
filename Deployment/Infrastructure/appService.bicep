targetScope = 'resourceGroup'

@description('Name of the application')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Location of the resource group to which to deploy to')
param location string

@description('Tags to apply to resources')
param tags object

@description('Resource ID of the App Service Plan')
param appServicePlanId string

@minValue(1)
param letterHintDelay int

@minValue(1)
param postRoundDelay int

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: 'cosmos-${appName}'
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: toLower('app-${appName}-${appEnv}')
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    httpsOnly: true
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      healthCheckPath: '/health'
      cors: {
        allowedOrigins: appEnv == 'prod' ? [ 'https://ohmyword.live' ] : [ 'https://test.ohmyword.live' ]
      }
      appSettings: [
        {
          name: 'Game__LetterHintDelay'
          value: string(letterHintDelay)
        }
        {
          name: 'Game__PostRoundDelay'
          value: string(postRoundDelay)
        }
        {
          name: 'CosmosDb__ConnectionString'
          value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
        {
          name: 'CosmosDb__DatabaseId'
          value: 'db-${appEnv}'
        }
        {
          name: 'CosmosDb__ContainerIds'
          value: '["players", "words"]'
        }
      ]
    }
  }
}
