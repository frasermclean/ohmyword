targetScope = 'resourceGroup'

@description('Name of the the Cosmos DB account.')
param cosmosDbAccountName string

@description('Name of the database')
param databaseName string

@description('Database request units per second.')
@minValue(400)
param throughput int

var containersDefinitions = [
  {
    name: 'definitions'
    partitionKeyPath: '/wordId'
  }
  {
    name: 'visitors'
    partitionKeyPath: '/id'
  }
  {
    name: 'words'
    partitionKeyPath: '/id'
  }
]

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
      throughput: throughput
    }
  }

  resource containers 'containers' = [for definition in containersDefinitions: {
    name: definition.name
    properties: {
      resource: {
        id: definition.name
        partitionKey: {
          paths: [
            definition.partitionKeyPath
          ]
        }
      }
    }
  }]
}

output databaseName string = database.name
