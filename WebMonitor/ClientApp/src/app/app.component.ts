import { Component, Signal, computed } from '@angular/core';
import { SysInfoService } from 'src/services/sys-info.service';
import { environment } from '../environments/environment';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  drawerOpen = false;
  showDebugWindow: Signal<boolean>;
  isDarkTheme: Signal<boolean>;

  toggleDrawer() {
    this.drawerOpen = !this.drawerOpen;
  }

  constructor(
    public sysInfo: SysInfoService,
    private appSettings: AppSettingsService
  ) {
    this.isDarkTheme = computed(() => this.appSettings.theme() === AppTheme.Dark);
    this.showDebugWindow = computed(() => this.appSettings.showDebugWindow());
  }
}
