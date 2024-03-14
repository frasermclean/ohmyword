targetScope = 'resourceGroup'

@description('Name of the application / workload')
param workload string

@description('Application environment')
@allowed([ 'prod', 'test' ])
param appEnv string

@description('Tags for the resources')
param tags object

@description('The default Azure location to deploy the resources to')
param location string = resourceGroup().location

@description('Apex domain name for the application')
param domainName string

@description('Container registry name')
param containerRegistryName string

@description('Container image name')
param containerImageName string

@description('Container image tag')
param containerImageTag string

@description('Shared resource group')
param sharedResourceGroup string

@description('Name of the Azure App Configuration instance')
param appConfigName string = '${workload}-shared-ac'

@description('If true, will attempt to create a managed certificate for the container app. Can be set to false if a managed certificate already exists.')
param createManagedCertficate bool = false

@description('Minimum number of container app replicas')
param containerAppMinReplicas int

@description('Maximum number of container app replicas')
param containerAppMaxReplicas int

@description('Frontend hostnames')
param frontEndHostnames array

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@allowed([
  '0.25'
  '0.5'
  '0.75'
  '1'
  '1.25'
  '1.5'
  '1.75'
  '2'
])
param containerAppCpuCores string = '0.5'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@allowed([
  '0.5'
  '1'
  '1.5'
  '2'
  '3'
  '3.5'
  '4'
])
param containerAppMemorySize string = '1'

var backendHostname = appEnv == 'prod' ? 'api.${domainName}' : 'test.api.${domainName}'
var containerAppName = '${workload}-${appEnv}-api-ca'
var certificateName = '${containerAppName}-cert'

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: '${workload}-shared-cae'
  scope: resourceGroup(sharedResourceGroup)

  resource managedCertificate 'managedCertificates' existing = {
    name: certificateName
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: '${workload}-shared-law'
  scope: resourceGroup(sharedResourceGroup)
}

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  name: '${workload}-shared-ag'
  scope: resourceGroup(sharedResourceGroup)
}

resource sharedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${workload}-shared-id'
  scope: resourceGroup(sharedResourceGroup)
}

// dns records for custom domain validation
module dnsRecords 'dnsRecords.bicep' = {
  name: 'dnsRecords-containerApp-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    appEnv: appEnv
    domainName: domainName
    containerAppIngressAddress: '${containerAppName}.${containerAppsEnvironment.properties.defaultDomain}'
    containerAppCustomDomainVerificationId: containerAppsEnvironment.properties.customDomainConfiguration.customDomainVerificationId
  }
}

// application insights
module appInsightsModule '../modules/appInsights.bicep' = {
  name: 'appInsights'
  params: {
    workload: workload
    category: appEnv
    location: location
    tags: tags
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    actionGroupId: actionGroup.id
  }
}

// container app
resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: containerAppName
  location: location
  tags: tags
  dependsOn: [ dnsRecords ]
  identity: {
    type: 'SystemAssigned,UserAssigned'
    userAssignedIdentities: {
      '${sharedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
        customDomains: [
          {
            name: backendHostname
            certificateId: createManagedCertficate ? null : containerAppsEnvironment::managedCertificate.id
            bindingType: createManagedCertficate ? 'Disabled' : 'SniEnabled'
          }
        ]
        corsPolicy: {
          allowCredentials: true
          maxAge: 600
          allowedOrigins: map(frontEndHostnames, (hostname) => 'https://${hostname}')
          allowedMethods: [ '*' ]
        }
      }
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: sharedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerImageName
          image: '${containerRegistryName}.azurecr.io/${containerImageName}:${containerImageTag}'
          resources: {
            cpu: json(containerAppCpuCores)
            memory: '${containerAppMemorySize}Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: appEnv
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsModule.outputs.connectionString
            }
            {
              name: 'APP_CONFIG_ENDPOINT'
              value: 'https://${appConfigName}.azconfig.io'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 80
              }
              initialDelaySeconds: 30
              periodSeconds: 60
            }
          ]
        }
      ]
      scale: {
        minReplicas: containerAppMinReplicas
        maxReplicas: containerAppMaxReplicas
      }
    }
  }
}

// container apps environment managed certificate
module managedCertificate 'managedCertificate.bicep' = if (createManagedCertficate) {
  name: 'managedCertificate-${appEnv}'
  scope: resourceGroup(sharedResourceGroup)
  dependsOn: [ containerApp ]
  params: {
    location: location
    containerAppsEnvironmentName: containerAppsEnvironment.name
    certificateName: certificateName
    customDomainFqdn: backendHostname
  }
}

output principalId string = containerApp.identity.principalId
