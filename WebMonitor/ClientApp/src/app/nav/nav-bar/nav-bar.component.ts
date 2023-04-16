import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent {
  @Output() menuToggleEvent = new EventEmitter<void>();

  onMenuToggle() {
    this.menuToggleEvent.emit();
  }
}
