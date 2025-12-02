import { Injectable, Signal, WritableSignal, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { OnDestroy } from '@angular/core';
import { appRoutes } from 'src/app/app-routes';
import { Route } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class RouteWatcherService implements OnDestroy {
  private subscription: Subscription;
  private _currentRoute: WritableSignal<Route> = signal(appRoutes[0]);
  // Prevent external mutation
  currentRoute: Signal<Route> = this._currentRoute;

  constructor(private router: Router) {
    this.subscription = router.events.subscribe((e) => {
      if (e instanceof NavigationEnd) {
        // Remove the leading slash
        const url = e.url.substring(1);
        const route = appRoutes.find(r => r.path === url);
        if (route) {
          this._currentRoute.set(route);
        } else {
          // TODO: Handle future cases with arguments
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
