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

  constructor(public appSettings: AppSettingsService) {
    this.isDarkTheme = computed(() => this.appSettings.settings().theme === AppTheme.Dark);
    this.showDebugWindow = computed(() => this.appSettings.settings().showDebugWindow);
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
