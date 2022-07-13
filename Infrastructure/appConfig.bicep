@description('Name of the application')
param appName string 

param location string = resourceGroup().location

@allowed([
  'Test'
  'Prod'
])
param environment string = 'Test'

param skuName string = 'standard'

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: 'AC-${appName}-${environment}'
  location: location
  sku: {
    name: skuName
  }
}
