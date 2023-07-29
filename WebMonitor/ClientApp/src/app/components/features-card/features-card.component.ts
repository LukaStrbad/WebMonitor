import { AfterViewInit, Component, Input } from '@angular/core';
import { SysInfoService } from "../../../services/sys-info.service";
import { SupportedFeatures } from "../../../model/supported-features";

@Component({
  selector: 'app-features-card',
  templateUrl: './features-card.component.html',
  styleUrls: ['./features-card.component.css']
})
export class FeaturesCardComponent implements AfterViewInit {
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
  featuresList: { name: string, supported: boolean, note?: string }[] = [];

  ngAfterViewInit() : void {
    this.featuresList = [
      { name: "CPU info", supported: this.features.cpuInfo },
      { name: "Memory info", supported: this.features.memoryInfo },
      { name: "Disk info", supported: this.features.diskInfo },
      { name: "CPU usage", supported: this.features.cpuUsage },
      { name: "Memory usage", supported: this.features.memoryUsage },
      { name: "Disk usage", supported: this.features.diskUsage },
      { name: "Network usage", supported: this.features.networkUsage },
      { name: "NVIDIA GPU usage", supported: this.features.nvidiaGpuUsage },
      { name: "AMD GPU usage", supported: this.features.amdGpuUsage },
      {
        name: "Intel GPU usage",
        supported: this.features.intelGpuUsage,
        note: "This feature is unsupported because the LibreHardwareMonitor library used by this app doesn't support Intel GPUs"
      },
      { name: "Processes", supported: this.features.processes },
      { name: "File browser", supported: this.features.fileBrowser },
      { name: "File download", supported: this.features.fileDownload },
      { name: "File upload", supported: this.features.fileUpload },
      {
        name: "NVIDIA refresh settings",
        supported: this.features.nvidiaRefreshSettings,
        note: "This feature is only supported on Windows because of high CPU usage on Windows on NVIDIA GPUs"
      },
      {
        name: "Battery info",
        supported: this.features.batteryInfo,
        note: "This feature is unsupported or the PC doesn't contain a battery"
      },
      { name: "Process priority", supported: this.features.processPriority },
      { name: "Process priority change", supported: this.features.processPriorityChange },
      { name: "Process affinity", supported: this.features.processAffinity },
      { name: "Terminal", supported: this.features.terminal },
    ].map(f => {
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
    })
  }
}
