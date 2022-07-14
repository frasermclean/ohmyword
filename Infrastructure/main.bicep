@description('Name of the application')
param appName string = 'OhMyWord'

@allowed([
  'Test'
  'Prod'
])
param environment string = 'Test'

@description('Location of the resource group in which to deploy')
param location string = resourceGroup().location

@description('Throughput of the database measured in R/U')
@minValue(400)
param databaseThroughput int = 1000

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower('DB-${appName}-${environment}') // cosmos db account names need to be lowercase
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: environment == 'Test' // enable free tier in test environment
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
  }
  resource database 'sqlDatabases@2022-05-15' = {
    name: appName
    properties: {
      resource: {
        id: appName
      }
      options: {
        throughput: databaseThroughput
      }
    }
  }
}

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
  resource cosmosDbDatabaseId 'keyValues@2022-05-01' = {
    name: 'CosmosDb:DatabaseId'
    properties: {
      value: appName
    }
  }
  resource cosmosDbEndpoint 'keyValues@2022-05-01' = {
    name: 'CosmosDb:Endpoint'
    properties: {
      value: cosmosDbAccount.properties.documentEndpoint
    }
  }
  resource cosmosDbPrimaryKey 'keyValues@2022-05-01' = {
    name: 'CosmosDb:PrimaryKey'
    properties: {
      value: cosmosDbAccount.listKeys().primaryMasterKey
    }
  }
}

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'ASP-${appName}-${environment}'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
  kind: 'linux'
}

// app service
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
          connectionString: appConfig.listKeys().value[2].connectionString
          type: 'Custom'
        }
      ]
    }
  }
}
