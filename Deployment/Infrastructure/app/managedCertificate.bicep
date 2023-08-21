param workload string
param location string = resourceGroup().location
param certificateName string
param customDomainFqdn string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: '${workload}-cae'

  resource managedCertificates 'managedCertificates' = {
    name: certificateName
    location: location
    properties: {
      subjectName: customDomainFqdn
      domainControlValidation: 'CNAME'
    }
  }
}
