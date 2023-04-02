targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('Category of the workload')
param category string

@description('The default Azure location to deploy the resources to')
param location string

@description('Name of the shared resource group')
param sharedResourceGroup string

@description('DNS domain name')
param domainName string

var tags = {
  workload: workload
  category: category
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: 'asp-${workload}-shared'
  scope: resourceGroup(sharedResourceGroup)
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: toLower('law-${workload}-shared')
  scope: resourceGroup(sharedResourceGroup)
}

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2022-09-01' existing = {
  name: toLower('vnet-${workload}')
  scope: resourceGroup(sharedResourceGroup)

  resource functionsSubnet 'subnets' existing = {
    name: 'FunctionsSubnet'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: toLower('${workload}shared')
  scope: resourceGroup(sharedResourceGroup)
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' existing = {
  name: 'kv-${workload}-shared'
  scope: resourceGroup(sharedResourceGroup)
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

// custom domain DNS records
module customDomain '../modules/customDomain.bicep' = {
  name: 'customDomain-${category}-${domainName}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    domainName: domainName
    subDomain: category
    verificationId: functionApp.properties.customDomainVerificationId
    hostNameOrIpAddress: '${functionApp.name}.azurewebsites.net'
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
    virtualNetworkSubnetId: virtualNetwork::functionsSubnet.id
    vnetRouteAllEnabled: true
    reserved: true
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|7.0'
      http20Enabled: true
      alwaysOn: true
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
        {
          name: 'Dictionary__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=DictionaryApiKey)'
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

// role assignment for function app to access storage account
module storageAccountRoleAssignment '../modules/roleAssignment.bicep' = {
  name: 'roleAssignment-${storageAccount.name}-${functionApp.name}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleNames: [ 'StorageTableDataContributor', 'KeyVaultSecretsUser' ]
  }
}
