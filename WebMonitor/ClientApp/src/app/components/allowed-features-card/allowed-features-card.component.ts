import { AfterViewInit, Component, Input } from '@angular/core';
import { AllowedFeatures } from 'src/model/allowed-features';

@Component({
  selector: 'app-allowed-features-card',
  templateUrl: './allowed-features-card.component.html',
  styleUrls: ['./allowed-features-card.component.css']
})
export class AllowedFeaturesCardComponent implements AfterViewInit {
  @Input({ required: true }) allowedFeatures!: AllowedFeatures;

  featuresList: AllowedFeatureData[] = [];

  ngAfterViewInit(): void {
    // Use push instead of assignment to avoid ExpressionChangedAfterItHasBeenCheckedError
    this.featuresList.push(...getAllowedFeaturesList(this.allowedFeatures));
  }
}

export interface AllowedFeatureData {
  name: string;
  allowed: boolean;
  featureName: keyof AllowedFeatures;
}

export function getAllowedFeaturesList(features: AllowedFeatures): AllowedFeatureData[] {
  return [
    { name: "CPU usage", allowed: features.cpuUsage, featureName: "cpuUsage" },
    { name: "Memory usage", allowed: features.memoryUsage, featureName: "memoryUsage" },
    { name: "Disk usage", allowed: features.diskUsage, featureName: "diskUsage" },
    { name: "Network usage", allowed: features.networkUsage, featureName: "networkUsage" },
    { name: "GPU usage", allowed: features.gpuUsage, featureName: "gpuUsage" },
    { name: "Refresh interval change", allowed: features.refreshIntervalChange, featureName: "refreshIntervalChange" },
    {
      name: "NVIDIA refresh settings",
      allowed: features.nvidiaRefreshSettings,
      featureName: "nvidiaRefreshSettings"
    },
    { name: "Processes", allowed: features.processes, featureName: "processes" },
    { name: "File browser", allowed: features.fileBrowser, featureName: "fileBrowser" },
    { name: "File download", allowed: features.fileDownload, featureName: "fileDownload" },
    { name: "File upload", allowed: features.fileUpload, featureName: "fileUpload" },
    { name: "Extended process info", allowed: features.extendedProcessInfo, featureName: "extendedProcessInfo" },
    {
      name: "Process priority change",
      allowed: features.processPriorityChange,
      featureName: "processPriorityChange"
    },
    {
      name: "Process affinity change",
      allowed: features.processAffinityChange,
      featureName: "processAffinityChange"
    },
    { name: "Terminal", allowed: features.terminal, featureName: "terminal" },
    { name: "Battery info", allowed: features.batteryInfo, featureName: "batteryInfo" },
  ];
}
