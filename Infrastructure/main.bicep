targetScope = 'resourceGroup'

@description('Name of the application')
param appName string = 'OhMyWord'

@allowed([
  'Test'
  'Prod'
])
param environment string = 'Test'

@description('Location of the resource group in which to deploy')
param location string = resourceGroup().location

// azure app configuration service
resource appConfig 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: 'AC-${appName}-${environment}'
  location: location
  sku: {
    name: 'standard'
  }
  resource letterHintDelay 'keyValues@2022-05-01' = {
    name: 'Game:LetterHintDelay'
    properties: {
      value: '3'
    }
  }
  resource postRoundDelay 'keyValues@2022-05-01' = {
    name: 'Game:PostRoundDelay'
    properties: {
      value: '5'
    }
  }
}

// get read only connection string from app config
var appConfigConnectionString = appConfig.listKeys().value[2].connectionString

module appService 'appService.bicep' = {
  name: 'appService'
  params: {
    appName: appName
    location: location
    environment: environment
    appConfigConnectionString: appConfigConnectionString
  }
}

module cosmosDb 'cosmosDb.bicep' = {
  name: 'cosmosDb'
  params: {
    appName: appName
    location: location
    environment: environment
  }
}
