import { AfterViewInit, Component, Input } from '@angular/core';
import { SupportedFeatures } from "../../../model/supported-features";
import { SysInfoService } from "../../../services/sys-info.service";

@Component({
  selector: 'app-supported-features-card',
  templateUrl: './supported-features-card.component.html',
  styleUrls: ['./supported-features-card.component.css']
})
export class SupportedFeaturesCardComponent implements AfterViewInit {
  featuresList: FeatureData[] = [];

  constructor(private sysInfo: SysInfoService) {
  }

  ngAfterViewInit(): void {
    this.init();
  }

  async init() {
    const supportedFeatures = await this.sysInfo.getSupportedFeatures();
    this.featuresList = getSupportedFeaturesList(supportedFeatures).map(f => {
      if (f.supported) {
        f.note = "This feature is supported";
      } else if (!f.note) {
        f.note = "This feature is unsupported on this system";
      }
      return f;
    }
    );
  }
}

interface FeatureData {
  name: string;
  supported: boolean;
  note?: string;
}

function getSupportedFeaturesList(features: SupportedFeatures): FeatureData[] {
  return [
    { name: "CPU info", supported: features.cpuInfo },
    { name: "Memory info", supported: features.memoryInfo },
    { name: "Disk info", supported: features.diskInfo },
    { name: "CPU usage", supported: features.cpuUsage },
    { name: "Memory usage", supported: features.memoryUsage },
    { name: "Disk usage", supported: features.diskUsage },
    { name: "Network usage", supported: features.networkUsage },
    { name: "NVIDIA GPU usage", supported: features.nvidiaGpuUsage },
    { name: "AMD GPU usage", supported: features.amdGpuUsage },
    {
      name: "Intel GPU usage",
      supported: features.intelGpuUsage,
      note: "This feature is unsupported because the LibreHardwareMonitor library used by this app doesn't support Intel GPUs"
    },
    { name: "Processes", supported: features.processes },
    { name: "File browser", supported: features.fileBrowser },
    { name: "File download", supported: features.fileDownload },
    { name: "File upload", supported: features.fileUpload },
    {
      name: "NVIDIA refresh settings",
      supported: features.nvidiaRefreshSettings,
      note: "This feature is only supported on Windows because of high CPU usage on Windows on NVIDIA GPUs"
    },
    {
      name: "Battery info",
      supported: features.batteryInfo,
      note: "This feature is unsupported or the PC doesn't contain a battery"
    },
    { name: "Process priority", supported: features.processPriority },
    { name: "Process priority change", supported: features.processPriorityChange },
    { name: "Process affinity", supported: features.processAffinity },
    { name: "Terminal", supported: features.terminal },
  ];
}


// function getSupportedFeaturesList(features: SupportedFeatures): FeatureData[] {
//   return [
//     { name: "CPU info", supported: features.cpuInfo, featureName: "cpuInfo" },
//     { name: "Memory info", supported: features.memoryInfo, featureName: "memoryInfo" },
//     { name: "Disk info", supported: features.diskInfo, featureName: "diskInfo" },
//     { name: "CPU usage", supported: features.cpuUsage, featureName: "cpuUsage" },
//     { name: "Memory usage", supported: features.memoryUsage, featureName: "memoryUsage" },
//     { name: "Disk usage", supported: features.diskUsage, featureName: "diskUsage" },
//     { name: "Network usage", supported: features.networkUsage, featureName: "networkUsage" },
//     { name: "NVIDIA GPU usage", supported: features.nvidiaGpuUsage, featureName: "nvidiaGpuUsage" },
//     { name: "AMD GPU usage", supported: features.amdGpuUsage, featureName: "amdGpuUsage" },
//     {
//       name: "Intel GPU usage",
//       supported: features.intelGpuUsage,
//       note: "This feature is unsupported because the LibreHardwareMonitor library used by this app doesn't support Intel GPUs",
//       featureName: "intelGpuUsage"
//     },
//     { name: "Processes", supported: features.processes, featureName: "processes" },
//     { name: "File browser", supported: features.fileBrowser, featureName: "fileBrowser" },
//     { name: "File download", supported: features.fileDownload, featureName: "fileDownload" },
//     { name: "File upload", supported: features.fileUpload, featureName: "fileUpload" },
//     {
//       name: "NVIDIA refresh settings",
//       supported: features.nvidiaRefreshSettings,
//       note: "This feature is only supported on Windows because of high CPU usage on Windows on NVIDIA GPUs",
//       featureName: "nvidiaRefreshSettings"
//     },
//     {
//       name: "Battery info",
//       supported: features.batteryInfo,
//       note: "This feature is unsupported or the PC doesn't contain a battery",
//       featureName: "batteryInfo"
//     },
//     { name: "Process priority", supported: features.processPriority, featureName: "processPriority" },
//     {
//       name: "Process priority change",
//       supported: features.processPriorityChange,
//       featureName: "processPriorityChange"
//     },
//     { name: "Process affinity", supported: features.processAffinity, featureName: "processAffinity" },
//     { name: "Terminal", supported: features.terminal, featureName: "terminal" },
//   ];
// }
