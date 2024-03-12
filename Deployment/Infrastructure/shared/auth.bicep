@description('Name of the application / workload')
param workload string

@description('The category of the workload')
param category string

@description('The default Azure location to deploy the resources to')
param location string = resourceGroup().location

@allowed([
  'United States'
  'Europe'
  'Asia Pacific'
  'Australia'
])
@description('Location of the B2C Tenant')
param b2cTenantLocation string

@description('Should the B2C Tenant be deployed?')
param deployB2CTenant bool = false

var tags = {
  workload: workload
  category: category
}

// b2c tenant (existing)
resource b2cTenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' = if (deployB2CTenant) {
  name: '${workload}auth.onmicrosoft.com'
  location: b2cTenantLocation
  tags: tags
  sku: {
    name: 'PremiumP1'
    tier: 'A0'
  }
  properties: {
    createTenantProperties: {
      countryCode: 'AU'
      displayName: 'OhMyWord B2C'
    }
  }
}

// application insights for b2c logging
module appInsights '../modules/appInsights.bicep' = {
  name: 'appInsights'
  params: {
    workload: workload
    category: category
    location: location
    actionGroupShortName: 'OMW-Auth'
  }
}

@description('Azure B2C Tenant ID')
output b2cTenantId string = b2cTenant.properties.tenantId
