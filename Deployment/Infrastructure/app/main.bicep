targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('The default Azure location to deploy the resources to')
param location string

@description('Apex domain name for the application')
param domainName string

@description('Database request units per second.')
@minValue(400)
param databaseThroughput int

@description('Database containers to create')
param databaseContainers array

@description('Location for the static web app')
param staticWebAppLocation string = 'centralus'

@secure()
@description('RapidAPI key')
param rapidApiKey string = ''

var sharedResourceGroup = 'rg-${appName}-shared'

var tags = {
  workload: appName
  category: 'app'
  environment: appEnv
}

var frontendHostname = appEnv == 'prod' ? domainName : 'test.${domainName}'
var backendHostname = appEnv == 'prod' ? 'api.${domainName}' : 'test.api.${domainName}'
var databaseId = '${appName}-${appEnv}'
var appConfigName = 'ac-${appName}-shared'

@description('Azure AD B2C client ID of single page application')
var authClientId = appEnv == 'prod' ? 'ee95c3c0-c6f7-4675-9097-0e4d9bca14e3' : '1f427277-e4b2-4f9b-97b1-4f47f4ff03c0'

@description('Azure AD B2C audience for API to validate')
var authAudience = appEnv == 'prod' ? '7a224ce3-b92f-4525-a563-a79856d04a78' : 'f1f90898-e7c9-40b0-8ebf-103c2b0b1e72'

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2022-09-01' existing = {
  name: 'vnet-${appName}'
  scope: resourceGroup(sharedResourceGroup)

  resource subnet 'subnets' existing = {
    name: 'snet-apps'
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: 'cosmos-${appName}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: 'asp-${appName}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'law-${appName}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: '${appName}shared'
  scope: resourceGroup(sharedResourceGroup)
}

// database
module database 'database.bicep' = {
  name: 'database-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseId: databaseId
    databaseContainers: databaseContainers
    databaseThroughput: databaseThroughput
    appServicePrincipalId: appService.identity.principalId
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('appi-${appName}-${appEnv}')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// app service
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: toLower('app-${appName}-${appEnv}')
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    httpsOnly: true
    serverFarmId: appServicePlan.id
    virtualNetworkSubnetId: virtualNetwork::subnet.id
    vnetRouteAllEnabled: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      healthCheckPath: '/health'
      http20Enabled: true
      ftpsState: 'Disabled'
      cors: {
        supportCredentials: true
        allowedOrigins: [ 'https://${frontendHostname}' ]
      }
      appSettings: [
        {
          name: 'AppConfig__Endpoint'
          value: 'https://${appConfigName}.azconfig.io'
        }
        {
          name: 'AppConfig__Environment'
          value: appEnv
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
        {
          name: 'CosmosDb__ContainerIds'
          value: string(map(databaseContainers, container => container.id))
        }
      ]
    }
  }

  resource hostNameBinding 'hostNameBindings' = {
    name: backendHostname
    properties: {
      siteName: appService.name
      hostNameType: 'Verified'
    }
  }
}

// diagnostic settings
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'diag-${appName}-${appEnv}'
  scope: appService
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceAuditLogs'
        enabled: true
      }
      {
        category: 'AppServiceIPSecAuditLogs'
        enabled: true
      }
      {
        category: 'AppServicePlatformLogs'
        enabled: true
      }
    ]
  }
}

// static web app
resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: 'swa-${appName}-${appEnv}'
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
    appName: appName
    appEnv: appEnv
    domainName: domainName
    appServiceVerificationId: appService.properties.customDomainVerificationId
    staticWebAppResourceId: staticWebApp.id
    staticWebAppDefaultHostname: staticWebApp.properties.defaultHostname
  }
}

// app service managed certificate
resource managedCertificate 'Microsoft.Web/certificates@2022-03-01' = {
  name: 'cert-${appName}-${appEnv}'
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlan.id
    canonicalName: appService::hostNameBinding.name
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
    principalId: appService.identity.principalId
    rapidApiKey: rapidApiKey
  }
}

// use module to enable hostname SNI binding
module sniEnable '../modules/sniEnable.bicep' = {
  name: 'sniEnable'
  params: {
    appServiceName: appService.name
    hostname: appService::hostNameBinding.name
    certificateThumbprint: managedCertificate.properties.thumbprint
  }
}

// role assignment for app service to access storage account
module storageAccountRoleAssignment '../modules/roleAssignments.bicep' = {
  name: 'roleAssignment-${storageAccount.name}-${appService.name}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    storageAccountName: storageAccount.name
    storageAccountRoles: [
      {
        principalId: appService.identity.principalId
        roleDefinitionId: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
      }
    ]
    serviceBusNamespaceRoles: [
      {
        principalId: appService.identity.principalId
        roleDefinitionId: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39' // Azure Service Bus Data Sender
      }
    ]
  }
}
