targetScope = 'resourceGroup'

@minLength(6)
@description('Name of the application / workload')
param workload string

@description('The category of the workload')
param category string

@description('The default Azure location to deploy the resources to')
param location string = resourceGroup().location

@description('Apex domain name for the application')
param domainName string

@minValue(400)
@description('Total throughput of the Cosmos DB account')
param databaseThroughput int

@description('Adminstrators Entra ID group')
param administratorsGroupId string = '9ea1ba97-5129-4176-a336-20af21b04b1f'

param b2cTenantId string

var tags = {
  workload: workload
  category: category
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

// user assigned identity
resource sharedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${workload}-${category}-id'
  location: location
  tags: tags
}

// cosmos db account
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: '${workload}-${category}-cosmos'
  location: location
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: true
    backupPolicy: {
      type: 'Continuous'
    }
    capacity: {
      totalThroughputLimit: databaseThroughput
    }
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      { locationName: location }
    ]
  }
}

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${workload}-${category}-law'
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

// action group
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: toLower('${workload}-${category}-ag')
  location: 'global'
  tags: tags
  properties: {
    enabled: true
    groupShortName: 'OMW Alerts'
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

// storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${workload}${category}'
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
  name: '${workload}-${category}-cae'
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
      {
        category: 'ContainerAppConsoleLogs'
        enabled: true
      }
    ]
  }
}

// app configuration
resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: '${workload}-${category}-ac'
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
      value: b2cTenantId
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
      value: b2cTenantId
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
  name: '${workload}-${category}-kv'
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
  name: '${workload}-${category}-sbns'
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

module roleAssignments 'roleAssignments.bicep' = {
  name: 'roleAssignments-shared'
  params: {
    principalId: administratorsGroupId
    keyVaultName: keyVault.name
    keyVaultRoles: [ 'Administrator' ]
    appConfigurationName: appConfiguration.name
    appConfigurationRoles: [ 'DataOwner' ]
    storageAccountName: storageAccount.name
    storageAccountRoles: [ 'TableDataContributor' ]
    serviceBusNamespaceName: serviceBusNamespace.name
    serviceBusNamespaceRoles: [ 'DataOwner' ]
  }
}

output storageAccountName string = storageAccount.name
output keyVaultName string = keyVault.name
output serviceBusNamespaceName string = serviceBusNamespace.name
output ipLookupQueueName string = serviceBusNamespace::devIpLookupQueue.name
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output actionGroupId string = actionGroup.id
