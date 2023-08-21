targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('The default Azure location to deploy the resources to')
param location string

@description('Category of the workload')
param category string

@description('DNS domain name')
param domainName string

@description('Name of the shared log analytics workspace')
param logAnalyticsWorkspaceName string

@description('Name of the shared storage account')
param sharedStorageAccountName string

@description('Shared resource group name')
param sharedResourceGroup string

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

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup(sharedResourceGroup)
}

// shared storage account
resource sharedStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: sharedStorageAccountName
  scope: resourceGroup(sharedResourceGroup)
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
  scope: resourceGroup(sharedResourceGroup)
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
  scope: resourceGroup(sharedResourceGroup)
}

// storage account for function app
resource functionAppStorageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: '${workload}${category}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('${workload}-${category}-appi')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// action group
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: toLower('${workload}-${category}-ag')
  location: 'global'
  tags: tags
  properties: {
    enabled: true
    groupShortName: 'SmartDetect'
    armRoleReceivers: [
      {
        name: 'Monitoring Contributor'
        roleId: '749f88d5-cbae-40b8-bcfc-e573ddc772fa'
        useCommonAlertSchema: true
      }
      {
        name: 'Monitoring Reader'
        roleId: '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
        useCommonAlertSchema: true
      }
    ]
  }
}

// smart detector alert rule
resource smartDetectorAlertRule 'Microsoft.AlertsManagement/smartDetectorAlertRules@2021-04-01' = {
  name: toLower('${workload}-${category}-fa-sdar')
  location: 'global'
  tags: tags
  properties: {
    description: 'Failure Anomalies notifies you of an unusual rise in the rate of failed HTTP requests or dependency calls.'
    severity: 'Sev3'
    state: 'Enabled'
    frequency: 'PT1M'
    detector: {
      id: 'FailureAnomaliesDetector'
    }
    scope: [
      appInsights.id
    ]
    actionGroups: {
      groupIds: [
        actionGroup.id
      ]
    }
  }
}

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${workload}-${category}-asp'
  location: location
  kind: 'linux'
  sku: {
    name: 'Y1'
    capacity: 1
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
  scope: resourceGroup(sharedResourceGroup)
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
    workspaceId: logAnalyticsWorkspace.id
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

module roleAssignments '../modules/roleAssignments.bicep' = if (attemptRoleAssignments) {
  name: 'roleAssignments-functions'
  scope: resourceGroup(sharedResourceGroup)
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
