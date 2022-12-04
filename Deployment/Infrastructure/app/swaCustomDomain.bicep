param hostname string
param staticWebAppName string

resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' existing = {
  name: staticWebAppName
}

resource staticWebAppCustomDomain 'Microsoft.Web/staticSites/customDomains@2022-03-01' = {
  name: hostname
  parent: staticWebApp
}
