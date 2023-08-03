export interface AllowedFeatures {
  cpuUsage: boolean;
  memoryUsage: boolean;
  diskUsage: boolean;
  networkUsage: boolean;
  gpuUsage: boolean;
  refreshIntervalChange: boolean;
  nvidiaRefreshSettings: boolean;
  processes: boolean;
  fileBrowser: boolean;
  fileDownload: boolean;
  fileUpload: boolean;
  extendedProcessInfo: boolean;
  processPriorityChange: boolean;
  processAffinityChange: boolean;
  terminal: boolean;
  batteryInfo: boolean;
}
