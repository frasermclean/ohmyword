targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('The default Azure location to deploy the resources to')
param location string

@description('Category of the workload')
param category string = 'functions'

@description('DNS domain name')
param domainName string

@description('Name of the shared log analytics workspace')
param logAnalyticsWorkspaceName string

@description('Name of the shared storage account')
param storageAccountName string

var tags = {
  workload: workload
  category: category
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('ai-${workload}-${category}')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${workload}-functions'
  location: location
  tags: tags
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

// function app
resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: toLower('func-${workload}-${category}')
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    reserved: true
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|7.0'
      http20Enabled: true
      ftpsState: 'Disabled'
      healthCheckPath: '/api/health'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
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
          name: 'TableService__Endpoint'
          value: 'https://${storageAccount.name}.table.${environment().suffixes.storage}'
        }
      ]
    }
  }

  // custom domain binding
  resource hostNameBinding 'hostNameBindings' = {
    name: '${category}.${domainName}'
    properties: {
      siteName: functionApp.name
      hostNameType: 'Verified'
    }
    dependsOn: [
      customDomain
    ]
  }
}

// custom domain DNS records
module customDomain '../modules/customDomain.bicep' = {
  name: 'customDomain-${category}-${domainName}'
  params: {
    domainName: domainName
    subDomain: category
    verificationId: functionApp.properties.customDomainVerificationId
    hostNameOrIpAddress: '${functionApp.name}.azurewebsites.net'
  }
}

// app service managed certificate
resource managedCertificate 'Microsoft.Web/certificates@2022-03-01' = {
  name: 'cert-${workload}-${category}'
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlan.id
    canonicalName: functionApp::hostNameBinding.name
  }
}

// use module to enable hostname SNI binding
module sniEnable '../modules/sniEnable.bicep' = {
  name: 'sniEnable'
  params: {
    appServiceName: functionApp.name
    hostname: functionApp::hostNameBinding.name
    certificateThumbprint: managedCertificate.properties.thumbprint
  }
}

var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, storageTableDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('The principal ID of the managed identity of function app')
output functionAppPrincipalId string = functionApp.identity.principalId

@description('Possible outbound IP addresses for the function app')
output functionAppOutboundIpAddresses array = split(functionApp.properties.possibleOutboundIpAddresses, ',')
