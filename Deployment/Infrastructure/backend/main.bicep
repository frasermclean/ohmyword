targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('The location of the resource group to deploy to')
param location string = resourceGroup().location

@description('Total throughput of the Cosmos DB account in measurement of Requests-per-Unit in the Azure Cosmos DB service')
param totalThroughputLimit int

var tags = {
  workload: appName
}

// cosmosDb account, database and containers
module cosmosDbAccount 'cosmosDbAccount.bicep' = {
  name: 'cosmosDbAccount'
  params: {
    name: toLower('cosmos-${appName}')
    location: location
    tags: tags
    totalThroughputLimit: totalThroughputLimit
  }
}
