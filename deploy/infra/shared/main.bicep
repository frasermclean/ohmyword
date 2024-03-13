targetScope = 'subscription'

@minLength(6)
@description('Name of the application / workload')
param workload string = 'ohmyword'

@description('The default Azure location to deploy the resources to')
param location string = 'australiaeast'

@description('Apex domain name for the application')
param domainName string

@description('Cosmos DB account total throughput')
param databaseThroughput int = 400

param attemptRoleAssignments bool = true

resource authResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${workload}-auth-rg'
  location: location
  tags: {
    workload: workload
    category: 'auth'
  }
}

resource sharedResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${workload}-shared-rg'
  location: location
  tags: {
    workload: workload
    category: 'shared'
  }
}

resource jobsResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${workload}-jobs-rg'
  location: location
  tags: {
    workload: workload
    category: 'jobs'
  }
}

module sharedDeployment 'shared.bicep' = {
  name: 'shared'
  scope: sharedResourceGroup
  params: {
    workload: workload
    category: 'shared'
    location: location
    domainName: domainName
    databaseThroughput: databaseThroughput
    b2cTenantId: authDeployment.outputs.b2cTenantId
  }
}

module authDeployment 'auth.bicep' = {
  name: 'auth'
  scope: authResourceGroup
  params: {
    workload: workload
    category: 'auth'
    location: location
    b2cTenantLocation: 'Australia'    
  }
}

module jobsDeployment 'functionsApp.bicep' = {
  name: 'functionsApp'
  scope: jobsResourceGroup
  params: {
    workload: workload
    category: 'jobs'
    location: location
    domainName: domainName
    sharedResourceGroup: sharedResourceGroup.name
    storageAccountName: sharedDeployment.outputs.storageAccountName
    keyVaultName: sharedDeployment.outputs.keyVaultName
    serviceBusNamespaceName: sharedDeployment.outputs.serviceBusNamespaceName
    ipLookupQueueName: sharedDeployment.outputs.ipLookupQueueName
    logAnalyticsWorkspaceId: sharedDeployment.outputs.logAnalyticsWorkspaceId
    actionGroupId: sharedDeployment.outputs.actionGroupId
    attemptRoleAssignments: attemptRoleAssignments
  }
}
