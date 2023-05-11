@description('The name of the key vault.')
param keyVaultName string = 'kv-ohmyword-shared'
param keyVaultRoles array = []

@description('The name of the storage account.')
param storageAccountName string = 'stohmywordshared'
param storageAccountRoles array = []

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
  name: keyVaultName
}

// key vault role assignments
resource keyVaultRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for item in keyVaultRoles: {
  name: guid(keyVault.id, item.roleDefinitionId, item.principalId)
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', item.roleDefinitionId)
    principalId: item.principalId
    principalType: 'ServicePrincipal'
  }
}]

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

// storage account role assignments
resource storageAccountRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for item in storageAccountRoles: {
  name: guid(storageAccount.id, item.roleDefinitionId, item.principalId)
  scope: storageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', item.roleDefinitionId)
    principalId: item.principalId
    principalType: 'ServicePrincipal'
  }
}]
