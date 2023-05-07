import { Component } from '@angular/core';
import { SysInfoService } from "../../services/sys-info.service";
import * as arrayHelpers from "../../helpers/array-helpers";
import * as numberHelpers from "../../helpers/number-helpers";
import { ComputerInfo } from 'src/model/computer-info';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  arrayHelpers = arrayHelpers;
  numberHelpers = numberHelpers;
  computerInfo?: ComputerInfo;

  constructor(public sysInfo: SysInfoService) {
    sysInfo.getComputerInfo()
      .then(computerInfo => this.computerInfo = computerInfo);
  }

  get osBuild(): string {
    if (this.sysInfo.data.computerInfo?.osBuild == null) {
      return "";
    }

    return `, Build: ${this.sysInfo.data.computerInfo.osBuild}`;
  }

  sizeDisplay(value: bigint): string {
    const options = new numberHelpers.MemoryByteOptions(true, false, 0);
    return numberHelpers.toByteString(value, options);
  }
}
