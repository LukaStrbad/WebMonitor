import { ChangeDetectorRef, Component, Signal, ViewEncapsulation, computed, effect } from '@angular/core';
import { SysInfoService } from 'src/services/sys-info.service';
import { AppSettingsService, AppTheme } from 'src/services/app-settings.service';
import { OverlayContainer } from '@angular/cdk/overlay';
import { UserService } from "../services/user.service";
import { Router } from "@angular/router";
import { MediaMatcher } from '@angular/cdk/layout';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class AppComponent {
  sidenavOpen = false;
  showDebugWindow: Signal<boolean>;
  isDarkTheme: Signal<boolean>;
  mobileQuery: MediaQueryList;

  toggleSidenav() {
    this.sidenavOpen = !this.sidenavOpen;
  }

  closeSidenav() {
    if (this.mobileQuery.matches) {
      this.sidenavOpen = false;
    }
  }

  constructor(
    public sysInfo: SysInfoService,
    private appSettings: AppSettingsService,
    overlayContainer: OverlayContainer,
    userService: UserService,
    router: Router,
    changeDetectorRef: ChangeDetectorRef,
    media: MediaMatcher
  ) {
    this.mobileQuery = media.matchMedia('(max-width: 768px)');
    this.mobileQuery.addEventListener('change', () => changeDetectorRef.detectChanges());

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
    });

    effect(async () => {
      if (!userService.authorized()) {
        this.sidenavOpen = false;
        await router.navigate(["/login"]);
        sysInfo.stopService();
      }
    });
  }
}
