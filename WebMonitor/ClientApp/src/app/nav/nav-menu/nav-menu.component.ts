import { menuRoutes } from 'src/app/app-routes';
import { RouteWatcherService } from 'src/services/route-watcher.service';
import { Route } from "@angular/router";
import { SysInfoService } from "../../../services/sys-info.service";
import { SupportedFeatures } from "../../../model/supported-features";
import { Component, EventEmitter, Output, effect } from "@angular/core";
import { UserService } from "../../../services/user.service";
import { AllowedFeatures } from 'src/model/allowed-features';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  menuRoutes = menuRoutes;
  @Output() onRouteSelected = new EventEmitter<void>();
  supportedFeatures?: SupportedFeatures;
  allowedFeatures?: AllowedFeatures;

  constructor(
    public routeWatcher: RouteWatcherService,
    private sysInfo: SysInfoService,
    private userService: UserService
  ) {
    effect(() => {
      if (this.userService.authorized()) {
        this.restartComponent();
      } else {
        this.supportedFeatures = undefined;
        this.allowedFeatures = undefined;
      }
    });
  }

  restartComponent() {
    this.sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);

    this.userService.requireUser()
      .then(user => this.allowedFeatures = user.allowedFeatures);
  }

  routeClicked() {
    this.onRouteSelected.emit();
  }

  getRouteName(route: Route) {
    if (route.path == "users" && this.userService.user?.isAdmin === true) {
      return "User/Admin";
    }
    return route.title;
  }

  shouldBeVisible(route: Route): boolean {
    if (!this.supportedFeatures || !this.allowedFeatures) {
      return false;
    }

    if (route.path === "usages") {
      return this.anyUsageAvailable();
    }
    if (route.path === "processes") {
      return (this.supportedFeatures.processes && this.allowedFeatures.processes) === true;
    }
    if (route.path === "file-browser") {
      return (this.supportedFeatures.fileBrowser && this.allowedFeatures.fileBrowser) === true;
    }
    if (route.path === "terminal") {
      return (this.supportedFeatures.terminal && this.allowedFeatures.terminal) === true;
    }
    if (route.path === "admin") {
      return this.userService.user?.isAdmin === true;
    }

    return true;
  }

  anyUsageAvailable(): boolean {
    return (this.supportedFeatures?.cpuUsage
      || this.supportedFeatures?.memoryUsage
      || this.supportedFeatures?.diskUsage
      || this.supportedFeatures?.networkUsage
      || this.supportedFeatures?.intelGpuUsage
      || this.supportedFeatures?.nvidiaGpuUsage
      || this.supportedFeatures?.amdGpuUsage) === true &&
      (this.allowedFeatures?.cpuUsage
        || this.allowedFeatures?.memoryUsage
        || this.allowedFeatures?.diskUsage
        || this.allowedFeatures?.networkUsage
        || this.allowedFeatures?.gpuUsage) === true;
  }
}
