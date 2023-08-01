targetScope = 'resourceGroup'

@description('Name of the application / workload')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Apex domain name for the application')
param domainName string

@description('Custom domain verification ID')
param customDomainVerificationId string

@description('Resource ID of the static web app')
param staticWebAppResourceId string

@description('Default hostname of the static web app')
param staticWebAppDefaultHostname string

@description('Custom domain verification token for the static web app')
param staticWebAppCustomDomainVerification string = ''

var isApex = appEnv == 'prod'
var apiCnameRecordName = appEnv == 'prod' ? 'api' : 'test.api'
var apiCnameRecordValue = 'app-${appName}-${appEnv}.azurewebsites.net'
var apiTxtRecordName = appEnv == 'prod' ? 'asuid.api' : 'asuid.test.api'
var swaRecordName = appEnv == 'prod' ? '@' : 'test'

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: domainName

  // custom domain verification record
  resource apiTxtRecord 'TXT' = {
    name: apiTxtRecordName
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

  // CNAME record for the app service API
  resource apiCnameRecord 'CNAME' = {
    name: apiCnameRecordName
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: apiCnameRecordValue
      }
    }
  }

  // TXT record for static web app custom domain validation
  resource staticWebAppTxtRecord 'TXT' = if (isApex && !empty(staticWebAppCustomDomainVerification)) {
    name: '@'
    properties: {
      TTL: 3600
      TXTRecords: [
        {
          value: [
            staticWebAppCustomDomainVerification
          ]
        }
      ]
    }
  }

  // A record for static web app (apex only)
  resource staticWebAppARecord 'A' = if (isApex) {
    name: '@'
    properties: {
      TTL: 3600
      targetResource: {
        id: staticWebAppResourceId
      }
    }
  }

  // CNAME record for the static web app (subdomain only)
  resource staticWebAppCnameRecord 'CNAME' = if (!isApex) {
    name: swaRecordName
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: staticWebAppDefaultHostname
      }
    }
  }
}
