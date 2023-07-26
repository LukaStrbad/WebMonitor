import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import { Route, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { menuRoutes } from 'src/app/app-routes';
import { RouteWatcherService } from 'src/services/route-watcher.service';
import { SysInfoService } from "../../../services/sys-info.service";
import { SupportedFeatures } from "../../../model/supported-features";
import { UserService } from "../../../services/user.service";

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent {
  @Output() menuToggleEvent = new EventEmitter<void>();
  menuRoutes = menuRoutes;
  supportedFeatures?: SupportedFeatures;

  constructor(
    public routeWatcher: RouteWatcherService,
    sysInfo: SysInfoService,
    public userService: UserService
  ) {
    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);
  }

  onMenuToggle() {
    this.menuToggleEvent.emit();
  }

  shouldBeHidden(route: Route): boolean {
    if (route.path === "file-browser") {
      return !this.supportedFeatures?.fileBrowser;
    } else if (route.path === "processes") {
      return !this.supportedFeatures?.processes;
    } else if (route.path === "terminal") {
      return !this.supportedFeatures?.terminal;
    } else {
      return false;
    }
  }
}
