import * as core from '@angular/core';
import { menuRoutes } from 'src/app/app-routes';
import { RouteWatcherService } from 'src/services/route-watcher.service';

@core.Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  menuRoutes = menuRoutes;
  @core.Output() onRouteSelected = new core.EventEmitter<void>();

  constructor(public routeWatcher: RouteWatcherService) { }

  routeClicked() {
    this.onRouteSelected.emit();
  }
}
