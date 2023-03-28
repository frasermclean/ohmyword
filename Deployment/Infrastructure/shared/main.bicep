targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('The default Azure location to deploy the resources to')
param location string

@description('Apex domain name for the application')
param domainName string

@description('Total throughput of the Cosmos DB account')
param totalThroughputLimit int

@description('Public IP addresses allowed to access Azure resources')
param allowedIpAddresses array

var tags = {
  workload: appName
  environment: 'shared'
}

var azurePortalIpAddresses = [
  '104.42.195.92'
  '40.76.54.131'
  '52.176.6.30'
  '52.169.50.45'
  '52.187.184.26'
]

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: domainName
  location: 'global'
  tags: tags
}

// virtual network
resource virtualNetwork 'Microsoft.Network/virtualNetworks@2022-09-01' = {
  name: 'vnet-${appName}'
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.3.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'AppServiceSubnet'
        properties: {
          addressPrefix: '10.3.1.0/24'
          serviceEndpoints: [
            { service: 'Microsoft.AzureCosmosDB' }
            { service: 'Microsoft.Storage' }
          ]
          delegations: [
            {
              name: 'dlg-serverFarms'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
    ]
  }

  resource appServiceSubnet 'subnets' existing = {
    name: 'AppServiceSubnet'
  }
}

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: 'cosmos-${appName}-shared'
  location: location
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    backupPolicy: {
      type: 'Continuous'
    }
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    capacity: {
      totalThroughputLimit: totalThroughputLimit
    }
    locations: [
      {
        locationName: location
      }
    ]
    isVirtualNetworkFilterEnabled: true
    virtualNetworkRules: [
      {
        id: virtualNetwork::appServiceSubnet.id
        ignoreMissingVNetServiceEndpoint: false
      }
    ]
    ipRules: [for ipAddress in concat(azurePortalIpAddresses, allowedIpAddresses): {
      ipAddressOrRange: ipAddress
    }]
  }
}

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${appName}-shared'
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('ai-${appName}-shared')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'st${appName}shared'
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          id: virtualNetwork::appServiceSubnet.id
          action: 'Allow'
        }
      ]
      ipRules: [for ipAddress in allowedIpAddresses: {
        value: ipAddress
        action: 'Allow'
      }]
    }
  }

  resource tableServices 'tableServices' = {
    name: 'default'
    resource usersTable 'tables' = {
      name: 'users'
    }
  }
}

// app service plan for the functions app (consumption plan)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${appName}-shared'
  location: location
  tags: tags
  kind: 'functionapp'
  sku: {
    name: 'Y1'
  }
  properties: {
    reserved: true
  }
}

// functions app
resource functionsApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'func-${appName}-shared'
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
}

// role definition for storage account
resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Blob Data Contributor
  scope: resourceGroup()
}

// assign role to storage account
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(functionsApp.id, roleDefinition.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: functionsApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
