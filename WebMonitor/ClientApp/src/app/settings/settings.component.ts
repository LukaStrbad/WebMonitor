import { Component, Signal, computed } from '@angular/core';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  isDarkTheme: Signal<boolean>;

  constructor(private appSettings: AppSettingsService) {
    this.isDarkTheme = computed(() => this.appSettings.theme() === AppTheme.Dark);
  }

  setDarkMode(value: boolean) {
    this.appSettings.setTheme(value ? AppTheme.Dark : AppTheme.Light);
  }

}
