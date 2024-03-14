targetScope = 'subscription'

@minLength(6)
@description('Name of the application / workload')
param workload string = 'ohmyword'

@description('Category of the workload')
param category string = 'app'

@description('The default Azure location to deploy the resources to')
param location string = 'australiaeast'

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string = 'test'

@description('Apex domain name for the application')
param domainName string = 'ohmyword.live'

@description('Shared resource group')
param sharedResourceGroup string = '${workload}-shared-rg'

@allowed([ 'centralus', 'eastus2', 'westus2', 'westeurope', 'eastasia' ])
@description('Location for the static web app')
param staticWebAppLocation string = 'centralus'

@description('Name of the Azure Container Registry')
param containerRegistryName string = 'snakebytecorecr'

@description('Minimum number of container app replicas')
param containerAppMinReplicas int = 0

@description('Maximum number of container app replicas')
param containerAppMaxReplicas int = 1

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int

@description('Azure AD audience')
param azureAdAudience string

@description('Azure AD client ID')
param azureAdClientId string

@description('Whether to attempt to assign roles to resources')
param attemptRoleAssignments bool = true

var tags = {
  workload: workload
  category: category
  environment: appEnv
}

// resource group
resource appResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${workload}-app-${appEnv}-rg'
  location: location
  tags: tags
}

// static web app
module staticWebAppModule 'staticWebApp.bicep' = {
  name: 'staticWebApp'
  scope: appResourceGroup
  params: {
    location: staticWebAppLocation
    tags: tags
    domainName: domainName
    sharedResourceGroupName: sharedResourceGroup
  }
}

// container app
module containerAppModule 'containerApp.bicep' = {
  scope: appResourceGroup
  name: 'containerApp'
  params: {
    workload: workload
    appEnv: appEnv
    location: location
    tags: tags
    domainName: domainName
    sharedResourceGroup: sharedResourceGroup
    containerRegistryName: containerRegistryName
    containerImageName: 'ohmyword-api'
    containerImageTag: 'latest'
    containerAppMinReplicas: containerAppMinReplicas
    containerAppMaxReplicas: containerAppMaxReplicas
    frontEndHostnames: staticWebAppModule.outputs.hostnames
  }
}

// database
module databaseModule 'database.bicep' = {
  name: 'database-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    databaseName: '${workload}-${appEnv}-db'
    databaseThroughput: databaseThroughput
    principalId: containerAppModule.outputs.principalId
  }
}

// signalR service
module signalRServiceModule 'signalRService.bicep' = {
  name: 'signalrService'
  scope: appResourceGroup
  params: {
    workload: workload
    appEnv: appEnv
    location: location
    tags: tags
    staticWebAppHostnames: staticWebAppModule.outputs.hostnames
    containerAppPrincipalId: containerAppModule.outputs.principalId
    attemptRoleAssignments: attemptRoleAssignments
  }
}

// app configuration settings
module appConfigModule 'appConfig.bicep' = {
  name: 'appConfig-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    appEnv: appEnv
    azureAdAudience: azureAdAudience
    azureAdClientId: azureAdClientId
    cosmosDbDatabaseId: databaseModule.outputs.databaseName
    signalRServiceHostname: signalRServiceModule.outputs.hostname
  }
}

// shared resource role assignments
module roleAssignmentsModule '../shared/roleAssignments.bicep' = if (attemptRoleAssignments) {
  name: 'roleAssignments-app-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    principalId: containerAppModule.outputs.principalId
    keyVaultRoles: [ 'SecretsUser' ]
    appConfigurationRoles: [ 'DataReader' ]
    storageAccountRoles: [ 'TableDataContributor' ]
    serviceBusNamespaceRoles: [ 'DataSender' ]
  }
}
