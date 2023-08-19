param workload string
param category string
param logAnalyticsWorkspaceName string
param sharedResourceGroup string

param location string = resourceGroup().location

var tags = {
  workload: workload
  category: category
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup(sharedResourceGroup)
}

// b2c tenant (existing)
resource b2cTenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' existing = {
  name: 'ohmywordauth.onmicrosoft.com'
}

// application insights for b2c logging
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

@description('Azure B2C Tenant ID')
output b2cTenantId string = b2cTenant.properties.tenantId
