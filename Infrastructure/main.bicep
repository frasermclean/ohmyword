@allowed([
  'prod'
  'dev'
])
param environment string = 'dev'

@description('Location of the resource group in which to deploy')
param location string = 'australiaeast'
