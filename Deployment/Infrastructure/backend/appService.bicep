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

@description('Linux app framework and version')
@allowed(['DOTNETCORE:7.0', 'DOTNETCORE:6.0'])
param linuxFxVersion string = 'DOTNETCORE:7.0'

param letterHintDelay int = 3

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: toLower('app-${appName}-${appEnv}')
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      appSettings: [
        {
          name: 'Game__LetterHintDelay'
          value: string(letterHintDelay)
        }
      ]
    }
  }
}
