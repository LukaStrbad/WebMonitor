import { Injectable, effect, signal } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppSettingsService {
  theme = signal(AppTheme.Light);

  constructor() {
    // Effect that reacts to theme changes and updates saved theme
    effect(() => localStorage.setItem("theme", this.theme()));

    // Load theme from local storage defaulting to light
    const savedTheme = (localStorage.getItem("theme") ?? AppTheme.Light) as AppTheme;
    this.setTheme(savedTheme);
  }

  setTheme(newTheme: AppTheme) {
    this.theme.set(newTheme);
  }
}

export enum AppTheme {
  Light = "light",
  Dark = "dark"
}
