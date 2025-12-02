export interface SupportedFeatures {
  cpuInfo: boolean;
  memoryInfo: boolean;
  diskInfo: boolean;
  cpuUsage: boolean;
  memoryUsage: boolean;
  diskUsage: boolean;
  networkUsage: boolean;
  nvidiaGpuUsage: boolean;
  amdGpuUsage: boolean;
  intelGpuUsage: boolean;
  processes: boolean;
  fileBrowser: boolean;
  fileDownload: boolean;
  fileUpload: boolean;
  nvidiaRefreshSettings: boolean;
  batteryInfo: boolean;
  processPriority: boolean;
  processPriorityChange: boolean;
  processAffinity: boolean;
  terminal: boolean;
}
