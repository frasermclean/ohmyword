targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Apex domain name for the application')
param domainName string

@description('Domain ownership ID')
param appServiceVerificationId string

@description('Default hostname of the static web app')
param staticWebAppDefaultHostname string

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: domainName
}

var apiCnameRecordName = appEnv == 'prod' ? 'api' : 'test.api'
var apiCnameRecordValue = 'app-${appName}-${appEnv}.azurewebsites.net'
var apiTxtRecordName = appEnv == 'prod' ? 'asuid.api' : 'asuid.test.api'
var staticWebAppCnameRecordName = appEnv == 'prod' ? '@' : 'test'

// CNAME record for the app service API
resource apiCnameRecord 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  name: apiCnameRecordName
  parent: dnsZone
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: apiCnameRecordValue
    }
  }
}

// TXT record for the app service API
resource apiTxtRecord 'Microsoft.Network/dnsZones/TXT@2018-05-01' = {
  name: apiTxtRecordName
  parent: dnsZone
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          appServiceVerificationId
        ]
      }
    ]
  }
}

resource staticWebAppCnameRecord 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  name: staticWebAppCnameRecordName
  parent: dnsZone
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: staticWebAppDefaultHostname
    }
  }
}
