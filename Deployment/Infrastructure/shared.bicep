param location string = 'australia'

resource tenant 'Microsoft.AzureActiveDirectory/b2cDirectories@2021-04-01' = {
  name: 'ohmywordb2c.onmicrosoft.com'
  location: location
  sku: {
    name: 'PremiumP1'
    tier: 'A0'
  }
  properties: {
    createTenantProperties: {
      countryCode: 'AU'
      displayName: 'Oh My Word B2C'
    }
  }
}
