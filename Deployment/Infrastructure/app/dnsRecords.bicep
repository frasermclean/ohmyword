targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Apex domain name for the application')
param domainName string

@description('Domain ownership ID')
param customDomainVerificationId string

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: domainName
}

var cnameRecordName = appEnv == 'prod' ? 'api' : 'test.api'
var cnameRecordValue = 'app-${appName}-${appEnv}.azurewebsites.net'
var txtRecordName = appEnv == 'prod' ? 'asuid.api' : 'asuid.test.api'

// CNAME record for the app service API
resource cnameRecord 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  name: cnameRecordName
  parent: dnsZone
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: cnameRecordValue
    }
  }
}

// TXT record for the app service API
resource txtRecord 'Microsoft.Network/dnsZones/TXT@2018-05-01' = {
  name: txtRecordName
  parent: dnsZone
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          customDomainVerificationId
        ]
      }
    ]
  }
}
