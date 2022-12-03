targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('The location of the resource group to deploy to')
param location string = resourceGroup().location

@description('Total throughput of the Cosmos DB account in measurement of Requests-per-Unit in the Azure Cosmos DB service')
param totalThroughputLimit int

@description('Apex domain name for the application')
param domainName string

@description('Application settings to be set for the web app')
param appSettings object

@description('Location to deploy static web app to')
param staticWebAppLocation string

@description('Custom domain name associated with the static web app')
param staticWebAppCustomDomainName string

var tags = {
  workload: appName
}

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: domainName
  location: 'global'
  tags: tags
}

// cosmosDb account, database and containers
module cosmosDbAccount 'cosmosDbAccount.bicep' = {
  name: 'cosmosDbAccount'
  params: {
    name: toLower('cosmos-${appName}')
    location: location
    tags: tags
    totalThroughputLimit: totalThroughputLimit
  }
}

// app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: toLower('asp-${appName}')
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

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: toLower('law-${appName}')
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

// production app service
module appService 'appService.bicep' = {
  name: 'appService'
  params: {
    appName: appName
    appEnv: 'prod'
    location: location
    tags: tags
    appServicePlanId: appServicePlan.id
    letterHintDelay: appSettings.letterHintDelay
    postRoundDelay: appSettings.postRoundDelay
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
  }
}

module staticWebApp 'staticWebApp.bicep' = {
  name: 'staticWebApp'
  params: {
    location: staticWebAppLocation
    appName: appName
    branch: 'main'
    repositoryUrl: 'https://github.com/frasermclean/ohmyword'
    customDomainName: staticWebAppCustomDomainName
  }
}
