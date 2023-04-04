targetScope = 'resourceGroup'

@description('Name of the the Cosmos DB account.')
param cosmosDbAccountName string

@description('ID of the database')
param databaseId string

@description('Containers to create in the database.')
param databaseContainers array

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int

@description('The principal ID of the app service. This is used to assign the database contributor role to the app service.')
param appServicePrincipalId string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-11-15' existing = {
  name: cosmosDbAccountName

  resource database 'sqlDatabases' = {
    name: databaseId
    properties: {
      resource: {
        id: databaseId
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
    name: guid(cosmosDbAccount.name, databaseId)
    properties: {
      roleName: '${databaseId} Contributor'
      type: 'CustomRole'
      assignableScopes: [
        '${cosmosDbAccount.id}/dbs/${databaseId}' // limit scope to the database
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
    name: guid(cosmosDbAccount.name, databaseId, appServicePrincipalId)
    properties: {
      roleDefinitionId: roleDefinition.id
      principalId: appServicePrincipalId
      scope: '${cosmosDbAccount.id}/dbs/${databaseId}'
    }
  }
}

output databaseName string = cosmosDbAccount::database.name
