import { Component } from '@angular/core';
import { SysInfoService } from "../../services/sys-info.service";
import * as arrayHelpers from "../../helpers/array-helpers";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  arrayHelpers = arrayHelpers;

  constructor(public sysInfo: SysInfoService) {
  }

  get osBuild(): string {
    if (this.sysInfo.data.computerInfo?.osBuild == null) {
      return "";
    }

    return `, Build: ${ this.sysInfo.data.computerInfo.osBuild }`;
  }

  protected readonly Math = Math;
}
