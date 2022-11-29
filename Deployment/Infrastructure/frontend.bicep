param appName string = 'ohmyword'

@allowed([ 'centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2' ])
param location string = 'centralus'

var tags = {
  workload: appName
}

resource swa 'Microsoft.Web/staticSites@2022-03-01' = {
  name: 'swa-${appName}'
  location: location
  tags: tags
  sku: {
    name: 'Free'
    size: 'Free'
  }
  properties: {
    repositoryUrl: 'https://github.com/frasermclean/ohmyword'
    branch: 'main'
  }
}
