using 'main.bicep'

param appEnv = 'test'

// container app
param containerAppMinReplicas = 0
param containerAppMaxReplicas = 1

// authentication
param azureAdAudience = ''
param azureAdClientId = ''

// database
param databaseThroughput = 600

param attemptRoleAssignments = bool(readEnvironmentVariable('ATTEMPT_ROLE_ASSIGNMENTS', 'true'))
