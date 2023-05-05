import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppSettingsService {
  private _theme: AppTheme;
  themeChanged = new Subject<AppTheme>();

  constructor() {
    // Load theme from local storage defaulting to light
    this._theme = (localStorage.getItem("theme") ?? AppTheme.Light) as AppTheme;
  }

  get theme(): AppTheme {
    return this._theme;
  }

  set theme(value: AppTheme) {
    this._theme = value;
    this.themeChanged.next(value);
    localStorage.setItem("theme", value);
  }
}

export enum AppTheme {
  Light = "light",
  Dark = "dark"
}
