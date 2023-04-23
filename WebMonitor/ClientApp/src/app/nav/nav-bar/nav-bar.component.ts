import { Component, EventEmitter, Output } from '@angular/core';
import { appRoutes } from 'src/app/app-routes';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent {
  @Output() menuToggleEvent = new EventEmitter<void>();
  appRoutes = appRoutes;

  onMenuToggle() {
    this.menuToggleEvent.emit();
  }
}
