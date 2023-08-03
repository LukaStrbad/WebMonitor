import { Injectable, WritableSignal, effect, signal } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AppSettingsService {
  /**
   * App settings theme.
   * Don't use set() method unless changing the whole object as it will not trigger updates.
   * To change a single property use the mutate() method.
   */
  settings: WritableSignal<AppSettings>;

  constructor() {
    // Load settings from local storage
    this.settings = signal(this.loadSettings());

    // Effect that reacts to changes in app settings
    effect(() => {
      // Save changed values to local storage
      this.saveSettings(this.settings());
    });
  }

  private saveSettings(value: AppSettings) {
    localStorage.setItem("appSettings", JSON.stringify(value));
  }

  private loadSettings(): AppSettings {
    const savedSettings = localStorage.getItem("appSettings");
    if (savedSettings) {
      return JSON.parse(savedSettings);
    } else {
      return DefaultAppSettings;
    }
  }
}

export enum AppTheme {
  Light,
  Dark
}

export interface GraphColors {
  cpu: string;
  memory: string;
  disk: string;
  network: string;
  networkUpload: string;
  gpu: string;
}

interface AppSettings {
  theme: AppTheme;
  showDebugWindow: boolean;
  graphColors: GraphColors;
}

const DefaultAppSettings: AppSettings = {
  theme: AppTheme.Light,
  // By default, show the debug window in development mode
  showDebugWindow: !environment.production,
  graphColors: {
    cpu: "teal",
    memory: "purple",
    disk: "green",
    network: "fuchsia",
    networkUpload: "red",
    gpu: "orange"
  }
};
