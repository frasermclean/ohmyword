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

@description('Resource ID for the log analytics workspace')
param logAnalyticsWorkspaceId string

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
        supportCredentials: true
        allowedOrigins: appEnv == 'prod' ? [ 'https://ohmyword.live' ] : [ 'https://test.ohmyword.live' ]
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
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

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('ai-${appName}-${appEnv}')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
} 
