targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('Category of the workload')
param category string

@description('The default Azure location to deploy the resources to')
param location string

@description('Name of the shared resource group')
param sharedResourceGroup string

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

resource functionsApp 'Microsoft.Web/sites@2022-03-01' = {
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
      ]
    }
  }
}

module storageAccountRoleAssignment '../modules/roleAssignment.bicep' = {
  name: 'roleAssignment-${storageAccount.name}-${functionsApp.name}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    principalId: functionsApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleName: 'StorageTableDataContributor'
  }
}
