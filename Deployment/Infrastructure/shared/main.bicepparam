using 'main.bicep'

param workload = 'ohmyword'
param location = 'australiaeast'
param domainName = 'ohmyword.live'
param totalThroughputLimit = 1000
param allowedIpAddresses = [
  '180.150.54.161'
]
