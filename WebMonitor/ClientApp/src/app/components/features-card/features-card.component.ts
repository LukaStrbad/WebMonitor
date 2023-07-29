import { AfterViewInit, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { SysInfoService } from "../../../services/sys-info.service";
import { AllowedFeatures, SupportedFeatures } from "../../../model/supported-features";

@Component({
  selector: 'app-features-card',
  templateUrl: './features-card.component.html',
  styleUrls: ['./features-card.component.css']
})
export class FeaturesCardComponent implements AfterViewInit, OnChanges {
  /**
   * The title of the card.
   */
  @Input() title: string = "Supported features";
  /**
   * Whether to show tooltips for the settings.
   */
  @Input({ required: true }) settingsTooltips!: boolean;
  /**
   * The text to show when a feature is supported.
   */
  @Input() positive = "Supported";
  /**
   * The text to show when a feature is not supported.
   */
  @Input() negative = "Not supported";
  /**
   * The supported features to display.
   */
  @Input({ required: true }) features!: SupportedFeatures;

  /**
   * Whether to compare features against the supported features.
   */
  @Input() showUnsupported = false;
  /**
   * The supported features to compare against.
   * This should contain only the features that are supported by the system.
   */
  @Input() supportedFeatures?: SupportedFeatures;

  featuresList: FeatureDataWithUnsupported[] = [];

  ngAfterViewInit(): void {
    const featuresList = getFeaturesList(this.features).map(f => {
        if (!this.settingsTooltips) {
          f.note = undefined;
          return f;
        }

        if (f.supported) {
          f.note = "This feature is supported";
        } else if (!f.note) {
          f.note = "This feature is unsupported or has been disabled";
        }
        return f;
      }
    );

    if (this.showUnsupported && this.supportedFeatures) {
      this.featuresList = featuresList.map(f => {
        const supported = this.supportedFeatures![f.featureName];
        return <FeatureDataWithUnsupported>{
          ...f,
          unsupported: !supported
        }
      });
      console.log(this.featuresList);
    } else {
      this.featuresList = featuresList;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // If the supported features have changed, re-render the view
    if (changes.showUnsupported || changes.supportedFeatures) {
      this.ngAfterViewInit();
    }
  }
}

export interface FeatureData {
  name: string;
  supported: boolean;
  note?: string;
}

export interface FeatureDataWithFeatureName extends FeatureData {
  featureName: keyof SupportedFeatures;
}

/**
 * An interface that allows for a third state of "unsupported" when the system does not support a feature.
 */
interface FeatureDataWithUnsupported extends FeatureData {
  unsupported?: boolean;
}

export function getFeaturesList(features: AllowedFeatures): FeatureDataWithFeatureName[] {
  return [
    { name: "CPU info", supported: features.cpuInfo, featureName: "cpuInfo" },
    { name: "Memory info", supported: features.memoryInfo, featureName: "memoryInfo" },
    { name: "Disk info", supported: features.diskInfo, featureName: "diskInfo" },
    { name: "CPU usage", supported: features.cpuUsage, featureName: "cpuUsage" },
    { name: "Memory usage", supported: features.memoryUsage, featureName: "memoryUsage" },
    { name: "Disk usage", supported: features.diskUsage, featureName: "diskUsage" },
    { name: "Network usage", supported: features.networkUsage, featureName: "networkUsage" },
    { name: "NVIDIA GPU usage", supported: features.nvidiaGpuUsage, featureName: "nvidiaGpuUsage" },
    { name: "AMD GPU usage", supported: features.amdGpuUsage, featureName: "amdGpuUsage" },
    {
      name: "Intel GPU usage",
      supported: features.intelGpuUsage,
      note: "This feature is unsupported because the LibreHardwareMonitor library used by this app doesn't support Intel GPUs",
      featureName: "intelGpuUsage"
    },
    { name: "Processes", supported: features.processes, featureName: "processes" },
    { name: "File browser", supported: features.fileBrowser, featureName: "fileBrowser" },
    { name: "File download", supported: features.fileDownload, featureName: "fileDownload" },
    { name: "File upload", supported: features.fileUpload, featureName: "fileUpload" },
    {
      name: "NVIDIA refresh settings",
      supported: features.nvidiaRefreshSettings,
      note: "This feature is only supported on Windows because of high CPU usage on Windows on NVIDIA GPUs",
      featureName: "nvidiaRefreshSettings"
    },
    {
      name: "Battery info",
      supported: features.batteryInfo,
      note: "This feature is unsupported or the PC doesn't contain a battery",
      featureName: "batteryInfo"
    },
    { name: "Process priority", supported: features.processPriority, featureName: "processPriority" },
    {
      name: "Process priority change",
      supported: features.processPriorityChange,
      featureName: "processPriorityChange"
    },
    { name: "Process affinity", supported: features.processAffinity, featureName: "processAffinity" },
    { name: "Terminal", supported: features.terminal, featureName: "terminal" },
  ];
}
