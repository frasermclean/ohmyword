using 'main.bicep'

param workload = 'ohmyword'
param location = 'australiaeast'
param domainName = 'ohmyword.live'
param totalThroughputLimit = 1000
param attemptRoleAssignments = true
param authResourceGroup = 'ohmyword-auth'
param functionsResourceGroup = 'ohmyword-functions'
