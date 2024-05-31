import { menuRoutes } from 'src/app/app-routes';
import { RouteWatcherService } from 'src/services/route-watcher.service';
import { Route } from "@angular/router";
import { SysInfoService } from "../../../services/sys-info.service";
import { SupportedFeatures } from "../../../model/supported-features";
import { Component, EventEmitter, Output, effect, OnDestroy } from "@angular/core";
import { UserService } from "../../../services/user.service";
import { AllowedFeatures } from 'src/model/allowed-features';
import { User } from 'src/model/user';
import { Subscription } from "rxjs";

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent implements OnDestroy {
  menuRoutes = menuRoutes;
  @Output() onRouteSelected = new EventEmitter<void>();
  supportedFeatures?: SupportedFeatures;
  me?: User;
  allowedFeatures?: AllowedFeatures;
  subscription: Subscription | undefined;

  constructor(
    public routeWatcher: RouteWatcherService,
    private sysInfo: SysInfoService,
    public userService: UserService
  ) {
    effect(() => {
      if (this.userService.authorized()) {
        this.restartComponent();
      } else {
        this.supportedFeatures = undefined;
        this.allowedFeatures = undefined;
      }
    });

    this.subscription = userService.allowedFeaturesChanged.subscribe(allowedFeatures => this.allowedFeatures = allowedFeatures);
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  restartComponent() {
    this.sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);

    this.userService.requireUser()
      .then(user => {
        this.me = user;
        this.allowedFeatures = user.allowedFeatures;
      });
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
      return this.supportedFeatures.processes && this.allowedFeatures.processes;
    }
    if (route.path === "file-browser") {
      return this.supportedFeatures.fileBrowser && this.allowedFeatures.fileBrowser;
    }
    if (route.path === "terminal") {
      return this.supportedFeatures.terminal && this.allowedFeatures.terminal;
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
