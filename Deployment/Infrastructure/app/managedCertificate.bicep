param location string = resourceGroup().location
param containerAppsEnvironmentName string
param certificateName string
param customDomainFqdn string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppsEnvironmentName

  resource managedCertificates 'managedCertificates' = {
    name: certificateName
    location: location
    properties: {
      subjectName: customDomainFqdn
      domainControlValidation: 'CNAME'
    }
  }
}
