targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Tags for the resources')
param tags object

@description('The default Azure location to deploy the resources to')
param location string = resourceGroup().location

@description('The hostnames of the static web app')
param staticWebAppHostnames array

@description('Prinicpal ID of the container app')
param containerAppPrincipalId string

@description('Whether to attempt to assign roles to resources')
param attemptRoleAssignments bool = false

// azure signalr service
resource signalrService 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: '${workload}-${appEnv}-sigr'
  location: location
  tags: tags
  kind: 'SignalR'
  sku: {
    name: 'Free_F1'
  }
  properties: {
    disableLocalAuth: true
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
    cors: {
      allowedOrigins: map(staticWebAppHostnames, (hostname) => 'https://${hostname}')
    }
  }
}

// signlar app server role definition
resource signalrAppServerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '420fcaa2-552c-430f-98ca-3264be4806c7'
}

// role assignment for signalr service
resource signalrServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (attemptRoleAssignments) {
  name: guid(signalrService.id, signalrAppServerRoleDefinition.id, containerAppPrincipalId)
  scope: signalrService
  properties: {
    principalId: containerAppPrincipalId
    roleDefinitionId: signalrAppServerRoleDefinition.id
  }
}

@description('The hostname of the SignalR service')
output hostname string = signalrService.properties.hostName
