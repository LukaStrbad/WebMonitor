import { Component } from '@angular/core';
import { appRoutes } from 'src/app/app-routes';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  appRoutes = appRoutes;
}
