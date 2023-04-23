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

var productionSubnetName = 'ProductionSubnet'
var testSubnetName = 'TestSubnet'

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
        name: productionSubnetName
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
      {
        name: testSubnetName
        properties: {
          addressPrefix: '10.3.2.0/24'
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

  resource productionSubnet 'subnets' existing = {
    name: productionSubnetName
  }

  resource testSubnet 'subnets' existing = {
    name: testSubnetName
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
        id: virtualNetwork::productionSubnet.id
        ignoreMissingVNetServiceEndpoint: false
      }
      {
        id: virtualNetwork::testSubnet.id
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
          id: virtualNetwork::productionSubnet.id
          action: 'Allow'
        }
        {
          id: virtualNetwork::testSubnet.id
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

// app service plan for app services and function apps
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-${workload}-shared'
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

// app configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: 'ac-${workload}-shared'
  location: location
  tags: tags
  sku: {
    name: 'Free'
  }
  identity: {
    type: 'SystemAssigned'
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
}

// key vault
resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' = {
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
          id: virtualNetwork::productionSubnet.id
          ignoreMissingVnetServiceEndpoint: false
        }
        {
          id: virtualNetwork::testSubnet.id
          ignoreMissingVnetServiceEndpoint: false
        }
      ]
      ipRules: [for ipAddress in allowedIpAddresses: {
        value: ipAddress
      }]
    }
  }
}

// key vault secrets user role definition
resource keyVaultSecretsUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: '4633458b-17de-408a-b874-0445c86b69e6'
  scope: resourceGroup()
}

// key vault secrets user role assignment
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, keyVaultSecretsUserRoleDefinition.id, appConfiguration.id)
  scope: keyVault
  properties: {
    principalId: appConfiguration.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: keyVaultSecretsUserRoleDefinition.id
  }
}

// application insights for b2c logging
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('ai-${workload}-auth')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
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
    storageAccountName: storageAccount.name
    sharedResourceGroup: resourceGroup().name
  }
}

module roleAssignments 'roleAssignments.bicep' = {
  name: 'roleAssignments-shared'
  params: {
    keyVaultName: keyVault.name
    storageAccountName: storageAccount.name
    storageAccountRoles: [
      {
        principalId: functions.outputs.functionAppPrincipalId
        roleDefinitionId: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
      }
    ]
  }
}
