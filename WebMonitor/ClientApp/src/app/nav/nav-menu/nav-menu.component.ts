import { menuRoutes } from 'src/app/app-routes';
import { RouteWatcherService } from 'src/services/route-watcher.service';
import { Route } from "@angular/router";
import { SysInfoService } from "../../../services/sys-info.service";
import { SupportedFeatures } from "../../../model/supported-features";
import { Component, EventEmitter, Output } from "@angular/core";
import { NavBarComponent } from "../nav-bar/nav-bar.component";

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  menuRoutes = menuRoutes;
  @Output() onRouteSelected = new EventEmitter<void>();
  supportedFeatures?: SupportedFeatures;

  constructor(
    public routeWatcher: RouteWatcherService,
    sysInfo: SysInfoService
  ) {
    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);
  }

  routeClicked() {
    this.onRouteSelected.emit();
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
