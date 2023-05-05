import { Component } from '@angular/core';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {

  constructor(private appSettings: AppSettingsService) { }

  setDarkMode(value: boolean) {
    this.appSettings.theme = value ? AppTheme.Dark : AppTheme.Light;
  }

  isDarkTheme(): boolean {
    return this.appSettings.theme === AppTheme.Dark;
  }

}
