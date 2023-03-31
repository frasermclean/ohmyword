@description('Name of the App Service.')
param appServiceName string

@description('Hostname to bind to')
param hostname string

@description('Thumbprint of the certificate to use for SSL')
param certificateThumbprint string

resource hostNameBinding 'Microsoft.Web/sites/hostNameBindings@2022-03-01' = {
  name: '${appServiceName}/${hostname}'
  properties: {
    sslState: 'SniEnabled'
    thumbprint: certificateThumbprint
  }
}
