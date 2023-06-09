import { Component, Signal, computed, effect } from '@angular/core';
import { NvidiaRefreshSetting } from 'src/model/nvidia-refresh-setting';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';
import { SysInfoService } from 'src/services/sys-info.service';
import { SupportedFeatures } from "../../model/supported-features";

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  isDarkTheme: Signal<boolean>;
  showDebugWindow: Signal<boolean>;
  /**
   * Rather than creating a separate component for each graph color setting, we can just create a list of them and use *ngFor.
   */
  graphColorSettings: GraphColorSetting[];
  nvidiaRefreshSettingEnum = NvidiaRefreshSetting;
  nvidiaRefreshSettingValues = Object
    .values(NvidiaRefreshSetting)
    .filter(n => typeof n === "number") as NvidiaRefreshSetting[];
  selectedNvidiaRefreshSetting = NvidiaRefreshSetting.Enabled;
  supportedFeatures?: SupportedFeatures;
  supportedFeaturesList: { name: string, supported: boolean, note?: string }[] = [];

  constructor(
    public appSettings: AppSettingsService,
    public sysInfo: SysInfoService
  ) {
    effect(() => this.selectedNvidiaRefreshSetting = sysInfo.nvidiaRefreshSettings().refreshSetting);
    this.isDarkTheme = computed(() => this.appSettings.settings().theme === AppTheme.Dark);
    this.showDebugWindow = computed(() => this.appSettings.settings().showDebugWindow);

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => {
        this.supportedFeatures = supportedFeatures;
        this.supportedFeaturesList = [
          { name: "CPU info", supported: supportedFeatures.cpuInfo },
          { name: "Memory info", supported: supportedFeatures.memoryInfo },
          { name: "Disk info", supported: supportedFeatures.diskInfo },
          { name: "CPU usage", supported: supportedFeatures.cpuUsage },
          { name: "Memory usage", supported: supportedFeatures.memoryUsage },
          { name: "Disk usage", supported: supportedFeatures.diskUsage },
          { name: "Network usage", supported: supportedFeatures.networkUsage },
          { name: "NVIDIA GPU usage", supported: supportedFeatures.nvidiaGpuUsage },
          { name: "AMD GPU usage", supported: supportedFeatures.amdGpuUsage },
          {
            name: "Intel GPU usage",
            supported: supportedFeatures.intelGpuUsage,
            note: "This feature is unsupported because the LibreHardwareMonitor library used by this app doesn't support Intel GPUs"
          },
          { name: "Processes", supported: supportedFeatures.processes },
          { name: "File browser", supported: supportedFeatures.fileBrowser },
          { name: "File download", supported: supportedFeatures.fileDownload },
          { name: "File upload", supported: supportedFeatures.fileUpload },
          {
            name: "NVIDIA refresh settings",
            supported: supportedFeatures.nvidiaRefreshSettings,
            note: "This feature is only supported on Windows because of high CPU usage on Windows on NVIDIA GPUs"
          },
          { name: "Battery info", supported: supportedFeatures.batteryInfo, note: "This feature is work in progress" }
        ].map(f => {
          if (f.supported) {
            f.note = "This feature is supported";
          } else if (!f.note) {
            f.note = "This feature is unsupported or has been disabled";
          }
          return f;
        })
      });

    this.graphColorSettings = [
      {
        label: "CPU graph color",
        color: this.appSettings.settings().graphColors.cpu,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.cpu = color)
      },
      {
        label: "Memory graph color",
        color: this.appSettings.settings().graphColors.memory,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.memory = color)
      },
      {
        label: "Disk graph color",
        color: this.appSettings.settings().graphColors.disk,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.disk = color)
      },
      {
        label: "Network graph color (download)",
        color: this.appSettings.settings().graphColors.network,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.network = color)
      },
      {
        label: "Network graph color (upload)",
        color: this.appSettings.settings().graphColors.networkUpload,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.networkUpload = color)
      },
      {
        label: "GPU graph color",
        color: this.appSettings.settings().graphColors.gpu,
        changeColor: (color: string) => appSettings.settings.mutate(s => s.graphColors.gpu = color)
      }
    ]
  }

  setDarkMode(value: boolean) {
    this.appSettings.settings.mutate(s => s.theme = value ? AppTheme.Dark : AppTheme.Light);
  }

  /**
   * Toggles the values of the showDebugWindow setting.
   */
  toggleDebugWindow() {
    this.appSettings.settings.mutate(s => s.showDebugWindow = !s.showDebugWindow);
  }

  nvidiaRefreshSettingName(refreshSetting: NvidiaRefreshSetting): string {
    switch (refreshSetting) {
      case NvidiaRefreshSetting.Enabled:
        return "Enabled";
      case NvidiaRefreshSetting.PartiallyDisabled:
        return "Partially disabled";
      case NvidiaRefreshSetting.Disabled:
        return "Disabled";
      case NvidiaRefreshSetting.LongerInterval:
        return "Longer interval";
    }
  }

  onNvidiaRefreshSettingChange() {
    this.sysInfo.nvidiaRefreshSettings.mutate(s => s.refreshSetting = this.selectedNvidiaRefreshSetting);
  }

  onNvidiaRefreshIntervalChange(target: EventTarget | null) {
    if (target == null) {
      return;
    }

    const value = parseInt((target as HTMLInputElement).value);
    this.sysInfo.nvidiaRefreshSettings.mutate(s => s.nRefreshIntervals = value);
  }

  onRefreshIntervalChange(target: EventTarget | null) {
    if (target == null) {
      return;
    }

    const value = parseInt((target as HTMLInputElement).value);
    this.sysInfo.updateRefreshInterval(value).then(ok => {
      if (ok) {
        this.sysInfo.data.refreshInfo.refreshInterval = value;
      }
    });
  }

}

/**
 * Represents a graph color setting
 */
interface GraphColorSetting {
  label: string;
  color: string;
  changeColor: (color: string) => void;
}
