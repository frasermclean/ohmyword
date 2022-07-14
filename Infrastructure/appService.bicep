@description('Name of the application')
param appName string = 'OhMyWord'

param location string = resourceGroup().location

@allowed([
  'Test'
  'Prod'
])
param environment string = 'Test'

@description('The SKU to use for the App Service plan')
param appServicePlanSkuName string = 'B1'

@description('Connection string for App Configuration service')
param appConfigConnectionString string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'ASP-${appName}-${environment}'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: appServicePlanSkuName
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: '${appName}-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      connectionStrings: [
        {
          name: 'AppConfig'
          connectionString: appConfigConnectionString
          type: 'Custom'
        }
      ]
    }
  }
}