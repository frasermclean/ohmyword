targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string = 'ohmyword'

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string = 'test'

@description('The default Azure location to deploy the resources to')
param location string = resourceGroup().location

@description('Apex domain name for the application')
param domainName string = 'ohmyword.live'

@description('Whether to attempt to assign roles to resources')
param attemptRoleAssignments bool = false

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int = 400

@description('Location for the static web app')
param staticWebAppLocation string = 'centralus'

@description('Name of the container registry')
param containerRegistryName string

@description('Container registry resource group')
param containerRegistryResourceGroup string

@description('Container image name')
param containerImageName string

@description('Container image tag')
param containerImageTag string

@description('Shared resource group')
param sharedResourceGroup string = '${workload}-shared'

var tags = {
  workload: workload
  category: 'app'
  environment: appEnv
}

var frontendHostname = appEnv == 'prod' ? domainName : 'test.${domainName}'
var appConfigName = '${workload}-ac'
var databaseId = '${workload}-${appEnv}'

@description('Azure AD B2C client ID of single page application')
var authClientId = appEnv == 'prod' ? 'ee95c3c0-c6f7-4675-9097-0e4d9bca14e3' : '1f427277-e4b2-4f9b-97b1-4f47f4ff03c0'

@description('Azure AD B2C audience for API to validate')
var authAudience = appEnv == 'prod' ? '7a224ce3-b92f-4525-a563-a79856d04a78' : 'f1f90898-e7c9-40b0-8ebf-103c2b0b1e72'

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: '${workload}-cosmos'
  scope: resourceGroup(sharedResourceGroup)
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: containerRegistryName
  scope: resourceGroup(containerRegistryResourceGroup)
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
  name: '${workload}-cae'
  scope: resourceGroup(sharedResourceGroup)
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: '${workload}-law'
  scope: resourceGroup(sharedResourceGroup)
}

resource sharedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${workload}-id'
  scope: resourceGroup(sharedResourceGroup)
}

// database
module database 'database.bicep' = {
  name: 'database-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseId: databaseId
    databaseThroughput: databaseThroughput
    principalId: containerApp.identity.principalId
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('${workload}-${appEnv}-appi')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// container app
resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: '${workload}-${appEnv}-ca'
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned,UserAssigned'
    userAssignedIdentities: {
      '${sharedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: sharedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerImageName
          image: '${containerRegistryName}.azurecr.io/${containerImageName}:${containerImageTag}'
          resources: {
            cpu: 1
            memory: '2Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: appEnv
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'APP_CONFIG_ENDPOINT'
              value: 'https://${appConfigName}.azconfig.io'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 80
              }
              initialDelaySeconds: 30
              periodSeconds: 60
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
      }
    }
  }
}

// static web app
resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: '${workload}-${appEnv}-swa'
  location: staticWebAppLocation
  tags: tags
  sku: {
    name: 'Free'
    size: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }

  // custom domain (subdomain in test)
  resource testCustomDomain 'customDomains' = if (appEnv == 'test') {
    name: frontendHostname
    dependsOn: [ dnsRecords ]
  }

  // custom domain (apex domain in prod)
  resource apexCustomDomain 'customDomains' = if (appEnv == 'prod') {
    name: frontendHostname
    dependsOn: [ dnsRecords ]
    properties: {
      validationMethod: 'dns-txt-token'
    }
  }
}

// dns records for custom domain validation
module dnsRecords 'dnsRecords.bicep' = {
  name: 'dnsRecords-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    appEnv: appEnv
    domainName: domainName
    containerAppIngressAddress: containerApp.properties.configuration.ingress.fqdn
    customDomainVerificationId: containerApp.properties.customDomainVerificationId
    staticWebAppResourceId: staticWebApp.id
    staticWebAppDefaultHostname: staticWebApp.properties.defaultHostname
  }
}

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
      allowedOrigins: [ 'https://${frontendHostname}' ]
    }
  }
}

module appConfig 'appConfig.bicep' = {
  name: 'appConfig-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    appConfigName: appConfigName
    appEnv: appEnv
    azureAdAudience: authAudience
    azureAdClientId: authClientId
    cosmosDbDatabaseId: databaseId
    signalRServiceHostname: signalrService.properties.hostName
  }
}

resource signalrAppServerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '420fcaa2-552c-430f-98ca-3264be4806c7'
}

// role assignment for signalr service
resource signalrServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (attemptRoleAssignments) {
  name: guid(signalrService.id, signalrAppServerRoleDefinition.id, containerApp.id)
  scope: signalrService
  properties: {
    principalId: containerApp.identity.principalId
    roleDefinitionId: signalrAppServerRoleDefinition.id
  }
}

// shared resource role assignments
module roleAssignments '../modules/roleAssignments.bicep' = if (attemptRoleAssignments) {
  name: 'roleAssignments-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    principalId: containerApp.identity.principalId
    keyVaultRoles: [ 'SecretsUser' ]
    appConfigurationRoles: [ 'DataReader' ]
    storageAccountRoles: [ 'TableDataContributor' ]
    serviceBusNamespaceRoles: [ 'DataSender' ]
  }
}
