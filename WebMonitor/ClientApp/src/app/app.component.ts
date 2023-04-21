import { Component } from '@angular/core';
import { SysInfoService } from 'src/services/sys-info.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  drawerOpen = false;
  showDebugWindow = !environment.production;

  toggleDrawer() {
    this.drawerOpen = !this.drawerOpen;
  }

  constructor(public sysInfo: SysInfoService) { }
}
