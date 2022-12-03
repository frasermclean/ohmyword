param appServiceName string
param hostname string
param certificateThumbprint string

resource hostNameBinding 'Microsoft.Web/sites/hostNameBindings@2022-03-01' = {
  name: '${appServiceName}/${hostname}'
  properties: {
    sslState: 'SniEnabled'
    thumbprint: certificateThumbprint
  }
}
