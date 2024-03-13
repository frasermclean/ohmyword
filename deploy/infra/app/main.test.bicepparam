using 'main.bicep'

param appEnv = 'test'

// container app
param containerAppMinReplicas = 0
param containerAppMaxReplicas = 1

// authentication
param azureAdClientId = '0fb40fba-41be-47e3-8c99-a875317649ec'
param azureAdAudience = ''

// database
param databaseThroughput = 400

param attemptRoleAssignments = bool(readEnvironmentVariable('ATTEMPT_ROLE_ASSIGNMENTS', 'true'))
