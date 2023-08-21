using 'main.bicep'

param appEnv = 'test'
param databaseThroughput = 400
param attemptRoleAssignments = true
param containerRegistryName = 'snakebyte'
param containerRegistryResourceGroup = 'rg-snakebyte-core'
param containerImageName = 'ohmyword-api'
param bindManagedCertificate = true
