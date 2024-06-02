import { Component, Signal, computed, effect } from '@angular/core';
import { NvidiaRefreshSetting } from 'src/model/nvidia-refresh-setting';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';
import { SysInfoService } from 'src/services/sys-info.service';
import { SupportedFeatures } from "../../model/supported-features";
import { UserService } from 'src/services/user.service';
import { AllowedFeatures } from 'src/model/allowed-features';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  isDarkTheme: Signal<boolean>;
  showInfoWindow: Signal<boolean>;
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
  allowedFeatures?: AllowedFeatures;

  constructor(
    public appSettings: AppSettingsService,
    public sysInfo: SysInfoService,
    userService: UserService
  ) {
    effect(() => {
      this.selectedNvidiaRefreshSetting = sysInfo.nvidiaRefreshSettings().refreshSetting;
      sysInfo.updateNvidiaRefreshSettings();
    });
    this.isDarkTheme = computed(() => this.appSettings.settings().theme === AppTheme.Dark);
    this.showInfoWindow = computed(() => this.appSettings.settings().showInfoWindow);

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => {
        this.supportedFeatures = supportedFeatures;
      });

    userService.requireUser()
      .then(user => this.allowedFeatures = user.allowedFeatures);

    this.graphColorSettings = [
      {
        label: "CPU graph color",
        color: this.appSettings.settings().graphColors.cpu,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.cpu = color;
          return clone;
        })
      },
      {
        label: "Memory graph color",
        color: this.appSettings.settings().graphColors.memory,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.memory = color;
          return clone;
        })
      },
      {
        label: "Disk graph color",
        color: this.appSettings.settings().graphColors.disk,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.disk = color;
          return clone;
        })
      },
      {
        label: "Network graph color (download)",
        color: this.appSettings.settings().graphColors.network,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.network = color;
          return clone;
        })
      },
      {
        label: "Network graph color (upload)",
        color: this.appSettings.settings().graphColors.networkUpload,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.networkUpload = color;
          return clone;
        })
      },
      {
        label: "GPU graph color",
        color: this.appSettings.settings().graphColors.gpu,
        changeColor: (color: string) => appSettings.settings.update(s => {
          let clone = Object.assign({}, s);
          clone.graphColors.gpu = color;
          return clone;
        })
      }
    ]
  }

  setDarkMode(value: boolean) {
    this.appSettings.settings.update(s => {
      let clone = Object.assign({}, s);
      clone.theme = value ? AppTheme.Dark : AppTheme.Light;
      return clone;
    });

    document.documentElement.classList.remove("dark-theme", "light-theme");

    const theme = value ? "dark-theme" : "light-theme";

    document.documentElement.classList.add(theme);
    const pageTheme = document.getElementById("page-theme");
    pageTheme?.setAttribute("href", `${theme}.css`);
  }

  /**
   * Toggles the values of the showDebugWindow setting.
   */
  toggleInfoWindow() {
    this.appSettings.settings.update(s => {
      let clone = Object.assign({}, s);
      clone.showInfoWindow = !s.showInfoWindow;
      return clone;
    });
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
    this.sysInfo.nvidiaRefreshSettings.update(s => {
      let clone = Object.assign({}, s);
      clone.refreshSetting = this.selectedNvidiaRefreshSetting;
      return clone;
    });
  }

  onNvidiaRefreshIntervalChange(target: EventTarget | null) {
    if (target == null) {
      return;
    }

    const value = parseInt((target as HTMLInputElement).value);
    this.sysInfo.nvidiaRefreshSettings.update(s => {
      let clone = Object.assign({}, s);
      clone.nRefreshIntervals = value;
      return clone;
    });
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
