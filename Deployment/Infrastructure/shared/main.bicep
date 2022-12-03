targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('The default Azure location to deploy the resources to')
param location string

@description('Apex domain name for the application')
param domainName string

@description('Total throughput of the Cosmos DB account')
param totalThroughputLimit int

var tags = {
  workload: appName
  environment: 'shared'
}

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: domainName
  location: 'global'
  tags: tags
}

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: 'cosmos-${appName}'
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

// production database
module databaseProd 'database.bicep' = {
  name: 'databaseProd'
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseName: 'db-prod'
    throughput: totalThroughputLimit / 5 * 3
  }
}

// test database
module databaseTest 'database.bicep' = {
  name: 'databaseTest'
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseName: 'db-test'
    throughput: totalThroughputLimit / 5 * 2
  }
}

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${appName}'
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: 'B1'
  }
  properties: {
    reserved: true
  }
}

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${appName}'
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}
