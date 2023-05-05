import { Injectable, effect, signal } from '@angular/core';
import { Subject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AppSettingsService {
  theme = signal(AppTheme.Light);
  showDebugWindow = signal<boolean>(false);
  appSettings: AppSettings;

  constructor() {
    // Load settings from local storage
    this.appSettings = this.loadSettings();
    this.theme.set(this.appSettings.theme);
    this.showDebugWindow.set(this.appSettings.showDebugWindow);

    // Effect that reacts to changes in app settings
    effect(() => {
      this.appSettings.theme = this.theme();
      this.appSettings.showDebugWindow = this.showDebugWindow();
      // Save changed values to local storage
      this.saveSettings(this.appSettings);
    });
  }

  saveSettings(value: AppSettings) {
    localStorage.setItem("appSettings", JSON.stringify(value));
  }

  loadSettings(): AppSettings {
    const savedSettings = localStorage.getItem("appSettings");
    if (savedSettings) {
      return JSON.parse(savedSettings);
    } else {
      return {
        theme: AppTheme.Light,
        // By default, show the debug window in development mode
        showDebugWindow: !environment.production
      };
    }
  }

  setTheme(newTheme: AppTheme) {
    this.theme.set(newTheme);
  }
}

export enum AppTheme {
  Light = "light",
  Dark = "dark"
}

interface AppSettings {
  theme: AppTheme;
  showDebugWindow: boolean;
}
