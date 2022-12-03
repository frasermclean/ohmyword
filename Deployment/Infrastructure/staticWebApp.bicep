param appName string

@allowed([ 'centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2' ])
param location string

@description('URL of the GitHub repository')
param repositoryUrl string

@secure()
@description('GitHub Personal Access Token')
param repositoryToken string = ''

@description('Git branch to use')
param branch string

var tags = {
  workload: toLower(appName)
}

resource swa 'Microsoft.Web/staticSites@2022-03-01' = {
  name: 'swa-${toLower(appName)}'
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
}

output defaultHostname string = swa.properties.defaultHostname
