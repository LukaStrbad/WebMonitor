import { Component } from '@angular/core';
import { menuRoutes } from 'src/app/app-routes';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  menuRoutes = menuRoutes;
}
