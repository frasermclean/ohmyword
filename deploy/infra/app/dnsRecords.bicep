targetScope = 'resourceGroup'

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Apex domain name for the application')
param domainName string

@description('Container app ingress address')
param containerAppIngressAddress string = ''

@description('Custom domain verification ID')
param containerAppCustomDomainVerificationId string = ''

@description('Resource ID of the static web app')
param staticWebAppResourceId string = ''

@description('Default hostname of the static web app')
param staticWebAppDefaultHostname string = ''

@description('Custom domain verification token for the static web app')
param staticWebAppCustomDomainVerification string = ''

// dns zone for the application
resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: domainName

  // custom domain verification record
  resource apiCustomDomainVerification 'TXT' = if (!empty(containerAppCustomDomainVerificationId)) {
    name: appEnv == 'prod' ? 'asuid.api' : 'asuid.test.api'
    properties: {
      TTL: 3600
      TXTRecords: [
        {
          value: [
            containerAppCustomDomainVerificationId
          ]
        }
      ]
    }
  }

  // CNAME record for the app service API
  resource apiCnameRecord 'CNAME' = if(!empty(containerAppIngressAddress)) {
    name: appEnv == 'prod' ? 'api' : 'test.api'
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: containerAppIngressAddress
      }
    }
  }

  // TXT record for static web app custom domain validation
  resource staticWebAppTxtRecord 'TXT' = if (appEnv == 'prod' && !empty(staticWebAppCustomDomainVerification)) {
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
  resource staticWebAppARecord 'A' = if (appEnv == 'prod' && !empty(staticWebAppResourceId)) {
    name: '@'
    properties: {
      TTL: 3600
      targetResource: {
        id: staticWebAppResourceId
      }
    }
  }

  // CNAME record for the static web app (subdomain only)
  resource staticWebAppCnameRecord 'CNAME' = if (appEnv == 'test' && !empty(staticWebAppDefaultHostname)) {
    name: 'test'
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: staticWebAppDefaultHostname
      }
    }
  }
}
