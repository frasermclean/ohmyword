targetScope = 'resourceGroup'

@description('Name of the application')
param appName string = 'OhMyWord'

@allowed([
  'Prod'
  'Dev'
])
param environment string = 'Dev'

@description('Location of the resource group in which to deploy')
param location string = resourceGroup().location

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'ASP-${appName}-${environment}'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'F1'
  }
  kind: 'linux'
}
