import { Component, OnDestroy } from '@angular/core';
import { SysInfoService } from 'src/services/sys-info.service';
import { environment } from '../environments/environment';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnDestroy {
  drawerOpen = false;
  showDebugWindow = !environment.production;
  isDarkTheme = false;
  subscription: Subscription;

  toggleDrawer() {
    this.drawerOpen = !this.drawerOpen;
  }

  constructor(
    public sysInfo: SysInfoService,
    private appSettings: AppSettingsService
  ) {
    this.isDarkTheme = this.appSettings.theme === AppTheme.Dark;

    this.subscription = appSettings.themeChanged.subscribe(theme => {
      this.isDarkTheme = theme === AppTheme.Dark;
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
