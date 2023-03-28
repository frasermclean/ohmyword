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

@description('Database containers to create')
param databaseContainers array

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

@description('Azure AD B2C client ID of single page application')
var authClientId = appEnv == 'prod' ? 'ee95c3c0-c6f7-4675-9097-0e4d9bca14e3' : '1f427277-e4b2-4f9b-97b1-4f47f4ff03c0'

@description('Azure AD B2C audience for API to validate')
var authAudience = appEnv == 'prod' ? '7a224ce3-b92f-4525-a563-a79856d04a78' : 'f1f90898-e7c9-40b0-8ebf-103c2b0b1e72'

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' existing = {
  name: 'cosmos-${appName}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'law-${appName}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource b2cTenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' existing = {
  name: 'ohmywordauth.onmicrosoft.com'
  scope: resourceGroup(sharedResourceGroup)
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: 'st${appName}shared'
  scope: resourceGroup(sharedResourceGroup)
}

// database
module database 'database.bicep' = {
  name: 'database-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    cosmosDbAccountName: cosmosDbAccount.name
    databaseName: '${appName}-${appEnv}'
    databaseContainers: databaseContainers
    databaseThroughput: databaseThroughput
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

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${appName}-${appEnv}'
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: 'B1'
  }
  properties: {
    reserved: true
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
          name: 'AzureAd__Instance'
          value: 'https://ohmywordauth.b2clogin.com'
        }
        {
          name: 'AzureAd__Domain'
          value: 'ohmywordauth.onmicrosoft.com'
        }
        {
          name: 'AzureAd__TenantId'
          value: b2cTenant.properties.tenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: authClientId
        }
        {
          name: 'AzureAd__Audience'
          value: authAudience
        }
        {
          name: 'AzureAd__SignUpSignInPolicyId'
          value: 'B2C_1A_SignUp_SignIn'
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
          value: database.outputs.databaseName
        }
        {
          name: 'CosmosDb__ContainerIds'
          value: string(map(databaseContainers, container => container.id))
        }
        {
          name: 'TableService__Endpoint'
          value: 'https://${storageAccount.name}.table.${environment().suffixes.storage}'
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

  // custom domain
  resource customDomain 'customDomains' = {
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

// use module to enable hostname SNI binding
module sniEnable 'sniEnable.bicep' = {
  name: 'sniEnable'
  params: {
    appServiceName: appService.name
    hostname: appService::hostNameBinding.name
    certificateThumbprint: managedCertificate.properties.thumbprint
  }
}
