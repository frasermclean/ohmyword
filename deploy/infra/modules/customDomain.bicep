/*
  App Service custom domain name
*/

targetScope = 'resourceGroup'

@description('Name of the existing DNS zone / domain name')
param domainName string

@description('Sub domain name (optional)')
param subDomain string = ''

@description('Custom Domain Verification ID')
param verificationId string

@description('Hostname or IP address. Needs to be an IP address if the domain is a root domain (e.g. contoso.com)')
param hostNameOrIpAddress string

var isApex = empty(subDomain)

resource dnsZone 'Microsoft.Network/dnszones@2018-05-01' existing = {
  name: domainName

  // verification TXT record 
  resource verificationRecord 'TXT' = {
    name: isApex ? 'asuid' : 'asuid.${subDomain}'
    properties: {
      TTL: 3600
      TXTRecords: [
        { value: [ verificationId ] }
      ]
    }
  }

  // A record if the domain is a root/apex domain
  resource aRecord 'A' = if (isApex) {
    name: '@'
    properties: {
      TTL: 3600
      ARecords: [
        { ipv4Address: hostNameOrIpAddress }
      ]
    }
  }

  // CNAME record for sub domains
  resource cnameRecord 'CNAME' = if (!isApex) {
    name: isApex ? '@' : subDomain
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: hostNameOrIpAddress
      }
    }
  }
}
