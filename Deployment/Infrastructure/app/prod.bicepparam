using 'main.bicep'

param appEnv = 'prod'
param databaseThroughput = 600
param attemptRoleAssignments = true
param containerRegistryName = 'snakebyte'
param containerRegistryResourceGroup = 'rg-snakebyte-core'
param containerImageName = 'ohmyword-api'
param containerImageTag = 'latest'
