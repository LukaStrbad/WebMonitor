import { Component, Signal, computed, effect } from '@angular/core';
import { SysInfoService } from 'src/services/sys-info.service';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';
import { OverlayContainer } from '@angular/cdk/overlay';

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
    private appSettings: AppSettingsService,
    overlayContainer: OverlayContainer
  ) {
    this.isDarkTheme = computed(() => this.appSettings.settings().theme === AppTheme.Dark);
    this.showDebugWindow = computed(() => this.appSettings.settings().showDebugWindow);

    // Effect that monitors isDarkTheme value and applies the appropriate theme class to the overlay container
    // Without this, dialogs will only appear in the default theme (light)
    effect(() => {
      const overlayContainerClasses = overlayContainer.getContainerElement().classList;
      // Find all classes that contain '-theme' and remove them
      const themeClassesToRemove = Array.from(overlayContainerClasses)
        .filter((item: string) => item.includes('-theme'));
      if (themeClassesToRemove.length > 0) {
        overlayContainerClasses.remove(...themeClassesToRemove);
      }

      // Add the appropriate theme class
      overlayContainerClasses.add(this.isDarkTheme() ? 'dark-theme' : 'light-theme');
    })
  }
}
