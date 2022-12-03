targetScope = 'resourceGroup'

@description('Name of the the Cosmos DB account')
param name string 

@description('The location of the resource group to deploy to')
param location string = resourceGroup().location

@description('Resource tags to apply')
param tags object

@description('Total throughput of the Cosmos DB account in measurement of Requests-per-Unit in the Azure Cosmos DB service')
@minValue(1000)
param totalThroughputLimit int

var prodDbThroughput = totalThroughputLimit / 5 * 3 // 60% of total throughput
var testDbThroughput = totalThroughputLimit / 5 * 2 // 40% of total throughput

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: name
  location: location
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    capacity: {
      totalThroughputLimit: totalThroughputLimit
    }
    locations: [
      {
        locationName: location        
      }
    ]
  }
}

// prod database
module cosmosDbDatabaseProd 'cosmosDbDatabase.bicep' = {
  name: 'cosmosDbDatabaseProd'
  params: {
    appEnv: 'prod'
    cosmosDbAccountName: cosmosDbAccount.name
    throughput: prodDbThroughput
  }
}

// test database
module cosmosDbDatabaseTest 'cosmosDbDatabase.bicep' = {
  name: 'cosmosDbDatabaseTest'
  params: {
    appEnv: 'test'
    cosmosDbAccountName: cosmosDbAccount.name
    throughput: testDbThroughput
  }
}
