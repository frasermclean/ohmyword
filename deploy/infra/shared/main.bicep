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

var deploymentSuffix = deployment().name

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

module sharedResourcesDeployment 'sharedResources.bicep' = {
  name: 'sharedResources-${deploymentSuffix}'
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
  name: 'auth-${deploymentSuffix}'
  scope: authResourceGroup
  params: {
    workload: workload
    category: 'auth'
    location: location
    b2cTenantLocation: 'Australia'    
  }
}

module jobsDeployment 'functionsApp.bicep' = {
  name: 'functionsApp-${deploymentSuffix}'
  scope: jobsResourceGroup
  params: {
    workload: workload
    category: 'jobs'
    location: location
    domainName: domainName
    sharedResourceGroup: sharedResourceGroup.name
    storageAccountName: sharedResourcesDeployment.outputs.storageAccountName
    keyVaultName: sharedResourcesDeployment.outputs.keyVaultName
    serviceBusNamespaceName: sharedResourcesDeployment.outputs.serviceBusNamespaceName
    ipLookupQueueName: sharedResourcesDeployment.outputs.ipLookupQueueName
    logAnalyticsWorkspaceId: sharedResourcesDeployment.outputs.logAnalyticsWorkspaceId
    actionGroupId: sharedResourcesDeployment.outputs.actionGroupId
    attemptRoleAssignments: attemptRoleAssignments
  }
}
