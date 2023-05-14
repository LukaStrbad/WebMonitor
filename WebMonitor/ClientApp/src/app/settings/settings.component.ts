import { Component, Signal, computed, signal } from '@angular/core';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';

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

  constructor(public appSettings: AppSettingsService) {
    this.isDarkTheme = computed(() => this.appSettings.settings().theme === AppTheme.Dark);
    this.showDebugWindow = computed(() => this.appSettings.settings().showDebugWindow);

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

}

/**
 * Represents a graph color setting
 */
interface GraphColorSetting {
  label: string;
  color: string;
  changeColor: (color: string) => void;
}
