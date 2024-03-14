targetScope = 'resourceGroup'

param workload string
param category string
param location string = resourceGroup().location
param tags object

@description('ID of an existing log analytics workspace to use. If not provided, a new log analytics workspace will be created.')
param logAnalyticsWorkspaceId string = ''

@description('ID of an existing action group to use. If not provided, a new action group will be created.')
param actionGroupId string = ''

@maxLength(12)
param actionGroupShortName string = 'ActionGroup'

// log analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = if (empty(logAnalyticsWorkspaceId)) {
  name: toLower('${workload}-${category}-law')
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

// action group
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = if (empty(actionGroupId)) {
  name: toLower('${workload}-${category}-ag')
  location: 'global'
  tags: tags
  properties: {
    enabled: true
    groupShortName: actionGroupShortName
    armRoleReceivers: [
      {
        name: 'Monitoring Contributor'
        roleId: '749f88d5-cbae-40b8-bcfc-e573ddc772fa'
        useCommonAlertSchema: true
      }
      {
        name: 'Monitoring Reader'
        roleId: '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
        useCommonAlertSchema: true
      }
    ]
  }
}

// application insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: toLower('${workload}-${category}-appi')
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: empty(logAnalyticsWorkspaceId) ? logAnalyticsWorkspace.id : logAnalyticsWorkspaceId
  }
}

// smart detector alert rule
resource smartDetectorAlertRule 'Microsoft.AlertsManagement/smartDetectorAlertRules@2021-04-01' = {
  name: toLower('${workload}-${category}-failures-sdar')
  location: 'global'
  tags: tags
  properties: {
    description: 'Failure Anomalies notifies you of an unusual rise in the rate of failed HTTP requests or dependency calls.'
    severity: 'Sev3'
    state: 'Enabled'
    frequency: 'PT1M'
    detector: {
      id: 'FailureAnomaliesDetector'
    }
    scope: [
      appInsights.id
    ]
    actionGroups: {
      groupIds: [
        empty(actionGroupId) ? actionGroup.id : actionGroupId
      ]
    }
  }
}

@description('Connection string to use to connect to the application insights instance.')
output connectionString string = appInsights.properties.ConnectionString
