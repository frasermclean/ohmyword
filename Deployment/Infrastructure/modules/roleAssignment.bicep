targetScope = 'resourceGroup'

@allowed([
  'Owner'
  'Contributor'
  'Reader'
  'StorageTableDataContributor'
  'StorageTableDataReader'
])
param roleName string

param principalId string

@allowed([
  'Device'
  'ForeignGroup'
  'Group'
  'ServicePrincipal'
  'User'
])
param principalType string

// Azure built-in roles: https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
@description('Mapping table of role names to role definition IDs.')
var roleDefinitionIds = {
  Owner: resourceId('Microsoft.Authorization/roleAssignments', '8e3af657-a8ff-443c-a75c-2fe8c4bcb635')
  Contributor: resourceId('Microsoft.Authorization/roleAssignments', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
  Reader: resourceId('Microsoft.Authorization/roleAssignments', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
  StorageTableDataContributor: resourceId('Microsoft.Authorization/roleAssignments', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
  StorageTableDataReader: resourceId('Microsoft.Authorization/roleAssignments', '76199698-9eea-4c19-bc75-cec21354c6b6')
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, roleDefinitionIds[roleName])
  properties: {
    roleDefinitionId: roleDefinitionIds[roleName]
    principalId: principalId
    principalType: principalType
  }
}