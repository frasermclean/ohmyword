targetScope = 'resourceGroup'

@minLength(6)
@description('Name of the application / workload')
param workload string = 'ohmyword'

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string = 'test'

@allowed([ 'centralus', 'eastus2', 'westus2', 'westeurope', 'eastasia' ])
@description('Location for the static web app')
param location string = 'centralus'

@description('Tags for the resources')
param tags object

@description('Apex domain name for the application')
param domainName string = 'ohmyword.live'

@description('Shared resource group')
param sharedResourceGroupName string = '${workload}-shared-rg'

// static web app
resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: '${workload}-${appEnv}-frontend-swa'
  location: location
  tags: tags
  sku: {
    name: 'Free'
    size: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }

  // custom domain
  resource customDomain 'customDomains' = if (appEnv == 'prod') {
    name: appEnv == 'prod' ? domainName : 'test.${domainName}'
    dependsOn: [ dnsRecords ]
    properties: {
      validationMethod: appEnv == 'prod' ? 'dns-txt-token' : null
    }
  }
}

module dnsRecords 'dnsRecords.bicep' = {
  name: 'dnsRecords-staticWebApp-${appEnv}'
  scope: resourceGroup(sharedResourceGroupName)
  params: {
    appEnv: appEnv
    domainName: domainName
    staticWebAppResourceId: staticWebApp.id
    staticWebAppDefaultHostname: staticWebApp.properties.defaultHostname
  }
}

@description('Hostnames at which the static web app is available')
output hostnames array = [
  staticWebApp.properties.defaultHostname
  staticWebApp::customDomain.name
]
