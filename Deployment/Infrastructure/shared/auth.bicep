param workload string
param category string
param logAnalyticsWorkspaceName string
param sharedResourceGroup string

@allowed([
  'United States'
  'Europe'
  'Australia'
  'Asia Pacific'
])
param b2cTenantLocation string

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
resource b2cTenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' = {
  name: 'ohmywordauth.onmicrosoft.com'
  location: b2cTenantLocation
  tags: tags
  sku: {
    name: 'PremiumP1'
    tier: 'A0'
  }
  properties: {
    createTenantProperties: {
      displayName: 'OhMyWord B2C'
      countryCode: 'AU'
    }
  }
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
