targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('The default Azure location to deploy the resources to')
param location string

@description('Apex domain name for the application')
param domainName string

@description('Total throughput of the Cosmos DB account')
param totalThroughputLimit int

@description('Public IP addresses allowed to access Azure resources')
param allowedIpAddresses array

var tags = {
  workload: workload
  environment: 'shared'
}

var azurePortalIpAddresses = [
  '104.42.195.92'
  '40.76.54.131'
  '52.176.6.30'
  '52.169.50.45'
  '52.187.184.26'
]

var appServiceSubnetName = 'snet-apps'

// b2c tenant (existing)
resource b2cTenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' existing = {
  name: 'ohmywordauth.onmicrosoft.com'
}

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: domainName
  location: 'global'
  tags: tags
}

// virtual network
resource virtualNetwork 'Microsoft.Network/virtualNetworks@2022-09-01' = {
  name: 'vnet-${workload}'
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
        name: appServiceSubnetName
        properties: {
          addressPrefix: '10.3.1.0/24'
          serviceEndpoints: [
            { service: 'Microsoft.AzureCosmosDB' }
            { service: 'Microsoft.KeyVault' }
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
    name: appServiceSubnetName
  }
}

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: 'cosmos-${workload}-shared'
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
  name: 'law-${workload}-shared'
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

// storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${workload}shared'
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
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

    resource getLocationsTable 'tables' = {
      name: 'geoLocations'
    }
  }
}

// app service plan for app services and function apps
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'asp-${workload}-shared'
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: 'B2'
  }
  properties: {
    reserved: true
  }
}

// app configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: 'ac-${workload}-shared'
  location: location
  tags: tags
  sku: {
    name: 'Free'
  }
  properties: {
    disableLocalAuth: false
  }

  resource azureAdInstance 'keyValues' = {
    name: 'AzureAd:Instance'
    properties: {
      value: 'https://ohmywordauth.b2clogin.com'
      contentType: 'text/plain'
    }
  }

  resource azureAdDomain 'keyValues' = {
    name: 'AzureAd:Domain'
    properties: {
      value: 'ohmywordauth.onmicrosoft.com'
      contentType: 'text/plain'
    }
  }

  resource azureAdTenantId 'keyValues' = {
    name: 'AzureAd:TenantId'
    properties: {
      value: b2cTenant.properties.tenantId
      contentType: 'text/plain'
    }
  }

  resource azureAdSignUpSignInPolicyId 'keyValues' = {
    name: 'AzureAd:SignUpSignInPolicyId'
    properties: {
      value: 'B2C_1A_SignUp_SignIn'
      contentType: 'text/plain'
    }
  }

  resource cosmosDbAccountEndpoint 'keyValues' = {
    name: 'CosmosDb:AccountEndpoint'
    properties: {
      value: cosmosDbAccount.properties.documentEndpoint
      contentType: 'text/plain'
    }
  }

  resource tableServiceEndpoint 'keyValues' = {
    name: 'TableService:Endpoint'
    properties: {
      value: storageAccount.properties.primaryEndpoints.table
      contentType: 'text/plain'
    }
  }

  resource gameLetterHintDelay 'keyValues' = {
    name: 'Game:LetterHintDelay'
    properties: {
      value: '3'
      contentType: 'text/plain'
    }
  }

  resource gamePostRoundDelay 'keyValues' = {
    name: 'Game:PostRoundDelay'
    properties: {
      value: '5'
      contentType: 'text/plain'
    }
  }

  resource serviceBusNamespaceKeyValue 'keyValues' = {
    name: 'ServiceBus:Namespace'
    properties: {
      value: '${serviceBusNamespace.name}.servicebus.windows.net'
      contentType: 'text/plain'
    }
  }

  resource serviceBusIpLookupDevQueuesKeyValues 'keyValues' = {
    name: 'ServiceBus:IpLookupQueueName$dev'
    properties: {
      value: 'dev-ip-lookup'
      contentType: 'text/plain'
    }
  }

  resource graphApiClientTenantIdKeyValue 'keyValues' = {
    name: 'GraphApiClient:TenantId'
    properties: {
      value: b2cTenant.properties.tenantId
      contentType: 'text/plain'
    }
  }

  resource graphApiClientClientIdKeyValue 'keyValues' = {
    name: 'GraphApiClient:ClientId'
    properties: {
      value: '5d698def-c925-41f4-897d-cfa10f0dd0c2'
      contentType: 'text/plain'
    }
  }

  resource graphApiClientClientSecretKeyValue 'keyValues' = {
    name: 'GraphApiClient:ClientSecret'
    properties: {
      value: '{"uri":"https://${keyVault.name}${environment().suffixes.keyvaultDns}/secrets/${keyVault::userReaderClientSecret.name}"}'
      contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    }
  }
}

// key vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: 'kv-${workload}-shared'
  location: location
  tags: tags
  properties: {
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: false
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          id: virtualNetwork::appServiceSubnet.id
          ignoreMissingVnetServiceEndpoint: false
        }
      ]
      ipRules: [for ipAddress in allowedIpAddresses: {
        value: ipAddress
      }]
    }
  }

  resource userReaderClientSecret 'secrets' existing = {
    name: 'user-reader-client-secret'
  }
}

// application insights for b2c logging
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('appi-${workload}-auth')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// service bus namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sbns-${workload}-shared'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
  }

  resource devIpLookupQueue 'queues' = {
    name: 'dev-ip-lookup'
  }

  resource sharedIpLookupQueue 'queues' = {
    name: 'shared-ip-lookup'
  }
}

module functions 'functions.bicep' = {
  name: 'functions'
  scope: resourceGroup('rg-${workload}-functions')
  params: {
    workload: workload
    location: location
    domainName: domainName
    logAnalyticsWorkspaceName: logAnalyticsWorkspace.name
    appServicePlanName: appServicePlan.name
    sharedResourceGroup: resourceGroup().name
    storageAccountName: storageAccount.name
    virtualNetworkSubnetId: virtualNetwork::appServiceSubnet.id
    serviceBusNamespaceName: serviceBusNamespace.name
    ipLookupQueueName: serviceBusNamespace::sharedIpLookupQueue.name
    keyVaultName: keyVault.name
  }
}

module roleAssignments '../modules/roleAssignments.bicep' = {
  name: 'roleAssignments-shared'
  params: {
    keyVaultName: keyVault.name
    keyVaultRoles: [
      {
        principalId: functions.outputs.functionAppPrincipalId
        roleDefinitionId: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User
      }
    ]
    storageAccountName: storageAccount.name
    storageAccountRoles: [
      {
        principalId: functions.outputs.functionAppPrincipalId
        roleDefinitionId: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
      }
    ]
    serviceBusNamespaceName: serviceBusNamespace.name
    serviceBusNamespaceRoles: [
      {
        principalId: functions.outputs.functionAppPrincipalId
        roleDefinitionId: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0' // Azure Service Bus Data Receiver
      }
    ]
  }
}
