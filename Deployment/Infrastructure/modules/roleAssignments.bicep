@description('The name of the key vault.')
param keyVaultName string = 'kv-ohmyword-shared'
param keyVaultRoles array = []

@description('The name of the storage account.')
param storageAccountName string = 'ohmywordshared'
param storageAccountRoles array = []

@description('The name of the service bus namespace.')
param serviceBusNamespaceName string = 'sbns-ohmyword-shared'
param serviceBusNamespaceRoles array = []

param containerRegistryName string = 'ohmyword'
param containerRegistryRoles array = []

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

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

// storage account role assignments
resource serviceBusNamespaceRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for item in serviceBusNamespaceRoles: {
  name: guid(serviceBusNamespace.id, item.roleDefinitionId, item.principalId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', item.roleDefinitionId)
    principalId: item.principalId
    principalType: 'ServicePrincipal'
  }
}]

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
}

// container registry role assignments
resource containerRegistryRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for item in containerRegistryRoles: {
  name: guid(containerRegistry.id, item.roleDefinitionId, item.principalId)
  scope: containerRegistry
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', item.roleDefinitionId)
    principalId: item.principalId
    principalType: 'ServicePrincipal'
  }
}]
