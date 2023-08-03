import { Component } from '@angular/core';
import { SysInfoService } from "../../services/sys-info.service";
import * as arrayHelpers from "../../helpers/array-helpers";
import * as numberHelpers from "../../helpers/number-helpers";
import { ComputerInfo } from 'src/model/computer-info';
import { SupportedFeatures } from "../../model/supported-features";
import { UserService } from 'src/services/user.service';
import { AllowedFeatures } from 'src/model/allowed-features';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  arrayHelpers = arrayHelpers;
  numberHelpers = numberHelpers;
  computerInfo?: ComputerInfo;
  supportedFeatures?: SupportedFeatures;
  allowedFeatures?: AllowedFeatures;

  constructor(
    public sysInfo: SysInfoService,
    userService: UserService
  ) {
    sysInfo.getComputerInfo()
      .then(computerInfo => this.computerInfo = computerInfo);

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);

    userService.requireUser()
      .then(user => this.allowedFeatures = user.allowedFeatures);
  }

  get osBuild(): string {
    if (this.sysInfo.data.computerInfo?.osBuild == null) {
      return "";
    }

    return `, Build: ${this.sysInfo.data.computerInfo.osBuild}`;
  }

  sizeDisplay(value: bigint | undefined): string {
    if (value == undefined) {
      return "";
    }
    const options = new numberHelpers.MemoryByteOptions(true, false, 0);
    return numberHelpers.toByteString(value, options);
  }
}
