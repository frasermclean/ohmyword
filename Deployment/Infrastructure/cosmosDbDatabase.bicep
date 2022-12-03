targetScope = 'resourceGroup'

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Name of the the Cosmos DB account.')
param cosmosDbAccountName string

@description('Database request units per second.')
@minValue(400)
param throughput int

var databaseName = 'db-${appEnv}'

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
  resource wordsContainer 'containers@2022-08-15' = {
    name: 'words'
    properties: {
      resource: {
        id: 'words'
        partitionKey: {
          paths: [
            '/partOfSpeech'
          ]
        }
        uniqueKeyPolicy: {
          uniqueKeys: [
            {
              paths: [
                '/value'
              ]
            }
          ]
        }
      }
    }
  }
  resource playersContainer 'containers@2022-08-15' = {
    name: 'players'
    properties: {
      resource: {
        id: 'players'
        partitionKey: {
          paths: [
            '/id'
          ]
        }
      }
    }
  }
}
