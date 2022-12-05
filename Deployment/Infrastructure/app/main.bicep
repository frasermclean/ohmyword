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
param databaseThroughput int

@description('Application specific settings')
param appSettings object

@description('Location for the static web app')
param staticWebAppLocation string = 'centralus'

var sharedResourceGroup = 'rg-${appName}-shared'

var tags = {
  workload: appName
  environment: appEnv
}

var frontendHostname = appEnv == 'prod' ? domainName : 'test.${domainName}'
var backendHostname = appEnv == 'prod' ? 'api.${domainName}' : 'test.api.${domainName}'

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

// database
module database 'database.bicep' = {
  name: 'database'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseName: 'db-${appEnv}'
    throughput: databaseThroughput
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('ai-${appName}-${appEnv}')
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
          name: 'Game__LetterHintDelay'
          value: string(appSettings.letterHintDelay)
        }
        {
          name: 'Game__PostRoundDelay'
          value: string(appSettings.postRoundDelay)
        }
        {
          name: 'CosmosDb__ConnectionString'
          value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
        {
          name: 'CosmosDb__DatabaseId'
          value: 'db-${appEnv}'
        }
        {
          name: 'CosmosDb__ContainerIds'
          value: '["players", "words"]'
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

  resource customDomains 'customDomains' = {
    name: frontendHostname
    dependsOn: [ dnsRecords ]
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

// use module to enable hostname SNI binding
module sniEnable 'sniEnable.bicep' = {
  name: 'sniEnable'
  params: {
    appServiceName: appService.name
    hostname: appService::hostNameBinding.name
    certificateThumbprint: managedCertificate.properties.thumbprint
  }
}
