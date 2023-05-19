import { Component, OnDestroy } from '@angular/core';
import { Route, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { menuRoutes } from 'src/app/app-routes';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnDestroy {
  menuRoutes = menuRoutes;
  subscription: Subscription;
  currentUrl: string = "";

  constructor(private router: Router) {
    this.subscription = router.events.subscribe((e) => {
      if ("url" in e) {
        // Remove the leading slash
        this.currentUrl = e["url"].substring(1);
      }
    })
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  isRouteSelected(route: Route) {
    if (!route.path) {
      return false;
    }
    return this.router.url.startsWith(route.path);
  }
}
