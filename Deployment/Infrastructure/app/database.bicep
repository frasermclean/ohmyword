targetScope = 'resourceGroup'

@description('Name of the the Cosmos DB account.')
param cosmosDbAccountName string

@description('Name of the database')
param databaseName string

@description('Containers to create in the database.')
param databaseContainers array

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: cosmosDbAccountName
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-08-15' = {
  name: databaseName
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: databaseName
    }
    options: {
      throughput: databaseThroughput
    }
  }

  resource containers 'containers' = [for container in databaseContainers: {
    name: container.id
    properties: {
      resource: {
        id: container.id
        partitionKey: {
          paths: [
            container.partitionKeyPath
          ]
        }
      }
    }
  }]
}

output databaseName string = database.name
