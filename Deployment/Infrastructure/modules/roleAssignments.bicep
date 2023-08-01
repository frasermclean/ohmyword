@description('The principal ID to assign the role to.')
param principalId string

@description('The name of the key vault.')
param keyVaultName string = 'kv-ohmyword-shared'
@allowed([ 'Administrator', 'SecretsUser' ])
param keyVaultRoles array = []

@description('The name of the app configuration service')
param appConfigurationName string = 'ac-ohmyword-shared'
@allowed([ 'DataOwner', 'DataReader' ])
param appConfigurationRoles array = []

@description('The name of the storage account.')
param storageAccountName string = 'ohmywordshared'
@allowed([ 'TableDataReader', 'TableDataContributor' ])
param storageAccountRoles array = []

@description('The name of the service bus namespace.')
param serviceBusNamespaceName string = 'sbns-ohmyword-shared'
@allowed([ 'DataOwner', 'DataReceiver', 'DataSender' ])
param serviceBusNamespaceRoles array = []

@description('The name of the container registry.')
param containerRegistryName string = 'ohmyword'
@allowed([ 'AcrPull', 'AcrPush', 'AcrDelete' ])
param containerRegistryRoles array = []

var keyVaultRoleIds = {
  Administrator: '00482a5a-887f-4fb3-b363-3b7fe8e74483'
  SecretsUser: '4633458b-17de-408a-b874-0445c86b69e6'
}

var appConfigurationRoleIds = {
  DataOwner: '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b'
  DataReader: '516239f1-63e1-4d78-a4de-a74fb236a071'
}

var storageAccountRoleIds = {
  TableDataReader: '76199698-9eea-4c19-bc75-cec21354c6b6'
  TableDataContributor: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

var serviceBusNamespaceRoleIds = {
  DataOwner: '090c5cfd-751d-490a-894a-3ce6f1109419'
  DataReceiver: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
  DataSender: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
}

var containerRegistryRoleIds = {
  AcrPull: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
  AcrPush: 'cdda3590-29a3-44f6-95f2-9f980659eb04'
  AcrDelete: 'c2f4ef07-c644-48eb-af81-4b1b4947fb11'
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
  name: keyVaultName
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigurationName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
}

// key vault role assignments
resource keyVaultRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in keyVaultRoles: {
  name: guid(keyVault.id, keyVaultRoleIds[role], principalId)
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', keyVaultRoleIds[role])
    principalId: principalId
  }
}]

// app configuration role assignments
resource appConfigurationRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in appConfigurationRoles: {
  name: guid(appConfiguration.id, appConfigurationRoleIds[role], principalId)
  scope: appConfiguration
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', appConfigurationRoleIds[role])
    principalId: principalId
  }
}]

// storage account role assignments
resource storageAccountRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in storageAccountRoles: {
  name: guid(storageAccount.id, storageAccountRoleIds[role], principalId)
  scope: storageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageAccountRoleIds[role])
    principalId: principalId
  }
}]

// storage account role assignments
resource serviceBusNamespaceRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in serviceBusNamespaceRoles: {
  name: guid(serviceBusNamespace.id, serviceBusNamespaceRoleIds[role], principalId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', serviceBusNamespaceRoleIds[role])
    principalId: principalId
  }
}]

// container registry role assignments
resource containerRegistryRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in containerRegistryRoles: {
  name: guid(containerRegistry.id, containerRegistryRoleIds[role], principalId)
  scope: containerRegistry
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', containerRegistryRoleIds[role])
    principalId: principalId
  }
}]
