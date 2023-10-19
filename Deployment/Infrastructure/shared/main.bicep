targetScope = 'resourceGroup'

@minLength(6)
@description('Name of the application / workload')
param workload string

@description('The default Azure location to deploy the resources to')
param location string

@description('Apex domain name for the application')
param domainName string

@minValue(400)
@description('Total throughput of the Cosmos DB account')
param totalThroughputLimit int

@description('Whether to attempt to assign roles to resources')
param attemptRoleAssignments bool

@description('Auth resource group name')
param authResourceGroup string

@description('Functions app resource group name')
param functionsResourceGroup string

var tags = {
  workload: workload
  category: 'shared'
}

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: domainName
  location: 'global'
  tags: tags

  resource dnsRecord 'A' = {
    name: '*.apps'
    properties: {
      TTL: 3600
      ARecords: [
        {
          ipv4Address: containerAppsEnvironment.properties.staticIp
        }
      ]
    }
  }

  resource txtRecord 'TXT' = {
    name: 'asuid.apps'
    properties: {
      TTL: 3600
      TXTRecords: [
        {
          value: [ containerAppsEnvironment.properties.customDomainConfiguration.customDomainVerificationId ]
        }
      ]
    }
  }
}

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: '${workload}-cosmos'
  location: location
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    backupPolicy: {
      type: 'Continuous'
    }
    capacity: {
      totalThroughputLimit: totalThroughputLimit
    }
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      { locationName: location }
    ]
  }
}

// user assigned identity
resource sharedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${workload}-id'
  location: location
  tags: tags
}

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${workload}-law'
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

  resource linkedStorageAccount 'linkedStorageAccounts@2020-08-01' = [for name in [ 'CustomLogs', 'Query', 'Alerts' ]: {
    name: name
    properties: {
      storageAccountIds: [
        storageAccount.id
      ]
    }
  }]
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
      defaultAction: 'Allow'
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

// container apps environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${workload}-cae'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'azure-monitor'
    }
  }
}

// container apps diagnostic settings
resource containerAppsEnvironmentLogging 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'log-analytics'
  scope: containerAppsEnvironment
  properties: {
    workspaceId: logAnalyticsWorkspace.id
    logs: [
      {
        category: 'ContainerAppSystemLogs'
        enabled: true
      }
    ]
  }
}

// app configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: '${workload}-ac'
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
      value: auth.outputs.b2cTenantId
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
      value: auth.outputs.b2cTenantId
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
  name: '${workload}-kv'
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
      defaultAction: 'Allow'
    }
  }

  resource userReaderClientSecret 'secrets' existing = {
    name: 'user-reader-client-secret'
  }
}

// service bus namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${workload}-sbns'
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

module auth 'auth.bicep' = {
  name: 'auth'
  scope: resourceGroup(authResourceGroup)
  params: {
    workload: workload
    category: 'auth'
    location: location
    sharedResourceGroup: resourceGroup().name
    logAnalyticsWorkspaceName: logAnalyticsWorkspace.name
    b2cTenantLocation: 'Australia'
  }
}

module functions 'functions.bicep' = {
  name: 'functions'
  scope: resourceGroup(functionsResourceGroup)
  params: {
    workload: workload
    category: 'functions'
    location: location
    domainName: domainName
    logAnalyticsWorkspaceName: logAnalyticsWorkspace.name
    sharedStorageAccountName: storageAccount.name
    sharedResourceGroup: resourceGroup().name
    serviceBusNamespaceName: serviceBusNamespace.name
    ipLookupQueueName: serviceBusNamespace::sharedIpLookupQueue.name
    keyVaultName: keyVault.name
    attemptRoleAssignments: attemptRoleAssignments
  }
}
