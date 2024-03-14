targetScope = 'resourceGroup'

@description('Name of the the Cosmos DB account.')
param cosmosDbAccountName string = 'ohmyword-shared-cosmos'

@description('Name / ID of the database')
param databaseName string

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int

@description('Principal ID used to assign the database contributor role to.')
param principalId string

var databaseContainers = [
  {
    id: 'words'
    partitionKeyPath: '/id'
  }
  {
    id: 'definitions'
    partitionKeyPath: '/wordId'
  }
  {
    id: 'players'
    partitionKeyPath: '/id'
  }
  {
    id: 'rounds'
    partitionKeyPath: '/sessionId'
  }
  {
    id: 'sessions'
    partitionKeyPath: '/id'
  }
]

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-11-15' existing = {
  name: cosmosDbAccountName

  resource database 'sqlDatabases' = {
    name: databaseName
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

  // custom role definition for the database
  resource roleDefinition 'sqlRoleDefinitions' = {
    name: guid(cosmosDbAccount.name, databaseName)
    properties: {
      roleName: '${databaseName} Contributor'
      type: 'CustomRole'
      assignableScopes: [
        '${cosmosDbAccount.id}/dbs/${databaseName}' // limit scope to the database
      ]
      permissions: [
        {
          dataActions: [
            'Microsoft.DocumentDB/databaseAccounts/readMetadata'
            'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
            'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/executeQuery'
          ]
        }
      ]
    }
  }

  // custom role assignment for the database
  resource roleAssignment 'sqlRoleAssignments' = {
    name: guid(cosmosDbAccount.name, databaseName, principalId)
    properties: {
      roleDefinitionId: roleDefinition.id
      principalId: principalId
      scope: '${cosmosDbAccount.id}/dbs/${databaseName}'
    }
  }
}

output databaseName string = cosmosDbAccount::database.name
