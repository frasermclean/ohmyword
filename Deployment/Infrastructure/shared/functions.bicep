targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('The default Azure location to deploy the resources to')
param location string

@description('Category of the workload')
param category string

@description('DNS domain name')
param domainName string

@description('Shared resource group name')
param sharedResourceGroupName string

@description('Name of the shared storage account')
param storageAccountName string

@description('Name of the service bus namespace')
param serviceBusNamespaceName string

@description('Name of the service bus queue for IP lookup')
param ipLookupQueueName string

@description('Name of the key vault')
param keyVaultName string

@description('Whether to attempt to assign roles to resources')
param attemptRoleAssignments bool

var tags = {
  workload: workload
  category: category
}

// shared storage account (existing)
resource sharedStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
  scope: resourceGroup(sharedResourceGroupName)
}

// shared key vault (existing)
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
  scope: resourceGroup(sharedResourceGroupName)
}

// service bus namespace (existing)
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
  scope: resourceGroup(sharedResourceGroupName)
}

// storage account for function app
resource functionAppStorageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: '${workload}${category}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

module appInsightsModule '../modules/appInsights.bicep' = {
  name: 'appInsights'
  params: {
    workload: workload
    category: category
    location: location
    actionGroupShortName: 'OMW-Func'
  }
}

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${workload}-${category}-asp'
  location: location
  tags: tags
  sku: {
    name: 'Y1'
  }
  properties: {
    reserved: true
  }
}

// function app
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: toLower('${workload}-${category}')
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|7.0'
      http20Enabled: true
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${functionAppStorageAccount.listKeys().keys[0].value}'
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
          value: appInsightsModule.outputs.connectionString
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
          value: 'https://${sharedStorageAccount.name}.table.${environment().suffixes.storage}'
        }
        {
          name: 'ServiceBus__FullyQualifiedNamespace'
          value: '${serviceBusNamespaceName}.servicebus.windows.net'
        }
        {
          name: 'ServiceBus__IpLookupQueueName'
          value: ipLookupQueueName
        }
        {
          name: 'RapidApi__ApiKey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=RapidApiKey-prod)'
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
  name: 'customDomain-${functionApp.name}'
  scope: resourceGroup(sharedResourceGroupName)
  params: {
    domainName: domainName
    subDomain: category
    verificationId: functionApp.properties.customDomainVerificationId
    hostNameOrIpAddress: '${functionApp.name}.azurewebsites.net'
  }
}

// app service managed certificate
resource managedCertificate 'Microsoft.Web/certificates@2022-03-01' = {
  name: '${workload}-${category}-cert'
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlan.id
    canonicalName: functionApp::hostNameBinding.name
  }
}

// diagnostic settings
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: functionApp.name
  scope: functionApp
  properties: {
    workspaceId: appInsightsModule.outputs.logAnalyticsWorkspaceId
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
      }
    ]
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

module roleAssignments 'roleAssignments.bicep' = if (attemptRoleAssignments) {
  name: 'roleAssignments-functions'
  scope: resourceGroup(sharedResourceGroupName)
  params: {
    principalId: functionApp.identity.principalId
    keyVaultName: keyVault.name
    keyVaultRoles: [ 'SecretsUser' ]
    storageAccountName: sharedStorageAccount.name
    storageAccountRoles: [ 'TableDataContributor' ]
    serviceBusNamespaceName: serviceBusNamespace.name
    serviceBusNamespaceRoles: [ 'DataReceiver' ]
  }
}
