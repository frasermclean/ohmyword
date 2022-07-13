@description('Name of the application')
param appName string = 'OhMyWord'

@description('Location of the resource group in which to deploy')
param location string = resourceGroup().location

@allowed([
  'Test'
  'Prod'
])
param environment string = 'Test'

@minValue(400)
param databaseThroughput int = 400

resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower('DB-${appName}-${environment}')
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: '${account.name}/${appName}'
  properties: {
    resource: {
      id: appName
    }
    options: {
      throughput: databaseThroughput
    }
  }
}
