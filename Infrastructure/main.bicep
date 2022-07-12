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

module appService 'appService.bicep' = {
  name: 'appService'
  params: {
    appName: appName
    location: location
    environment: environment
  }
}
