@description('Name of the application / workload')
param appName string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@allowed([ 'centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2' ])
param location string = 'centralus'

@description('URL of the GitHub repository')
param repositoryUrl string

@secure()
@description('GitHub Personal Access Token')
param repositoryToken string = ''

@description('Git branch to use')
param branch string

param customDomainName string

var tags = {
  workload: toLower(appName)
  environment: toLower(appEnv)
}

resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: toLower('swa-${(appName)}-${appEnv}')
  location: location
  tags: tags
  sku: {
    name: 'Free'
    size: 'Free'
  }
  properties: {
    repositoryUrl: repositoryUrl
    repositoryToken: repositoryToken
    branch: branch
    buildProperties: {
      githubActionSecretNameOverride: 'AZURE_STATIC_WEB_APPS_API_TOKEN'
      skipGithubActionWorkflowGeneration: true
    }
  }

  resource customDomains 'customDomains' = {
    name: customDomainName
  }
}

output defaultHostname string = staticWebApp.properties.defaultHostname
