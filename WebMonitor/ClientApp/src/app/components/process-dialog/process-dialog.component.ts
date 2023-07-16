import { AfterViewInit, Component, Inject, LOCALE_ID, OnDestroy } from '@angular/core';
import { ProcessInfo } from "../../../model/process-info";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { ExtendedProcessInfo } from "../../../model/extended-process-info";
import { SysInfoError, SysInfoService } from "../../../services/sys-info.service";
import { Subscription } from "rxjs";
import { ProcessPriorityWin } from "../../../model/process-priority";
import * as numberHelpers from "../../../helpers/number-helpers";
import { SupportedFeatures } from "../../../model/supported-features";

@Component({
  selector: 'app-process-dialog',
  templateUrl: './process-dialog.component.html',
  styleUrls: ['./process-dialog.component.css']
})
export class ProcessDialogComponent implements OnDestroy, AfterViewInit {
  extendedInfo: ExtendedProcessInfo | null = null;
  affinityThreads: boolean[] | null = null;
  subscription?: Subscription;
  error?: string;
  supportedFeatures?: SupportedFeatures;

  numberHelpers = numberHelpers;

  constructor(
    public dialogRef: MatDialogRef<ProcessDialogComponent>,
    private sysInfo: SysInfoService,
    @Inject(MAT_DIALOG_DATA) public data: ProcessDialogData,
    @Inject(LOCALE_ID) public locale: string
  ) {
    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);
  }

  onCloseClick() {
    this.dialogRef.close();
  }

  ngAfterViewInit(): void {
    this.sysInfo.getExtendedProcessInfo(this.data.processInfo.pid).then(async info => {
      this.extendedInfo = info;

      const computerInfo = await this.sysInfo.getComputerInfo();
      if (!computerInfo.cpu || !this.supportedFeatures?.processAffinity) {
        return;
      }

      let binaryString = info.affinity.toString(2);

      this.affinityThreads = [];

      for (let i = 0; i < binaryString.length; i++) {
        if (binaryString[binaryString.length - 1 - i] === '1') {
          this.affinityThreads.push(true);
        } else {
          this.affinityThreads.push(false);
        }
      }

    });

    this.subscription = this.sysInfo.errorEmitter.subscribe(message => {
      const [type, msg] = message;
      if (type === SysInfoError.ExtendedProcessInfo) {
        this.error = msg;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  processPriorityName(priority: ProcessPriorityWin) {
    switch (priority) {
      case ProcessPriorityWin.Realtime:
        return 'Real time';
      case ProcessPriorityWin.High:
        return 'High';
      case ProcessPriorityWin.AboveNormal:
        return 'Above normal';
      case ProcessPriorityWin.Normal:
        return 'Normal';
      case ProcessPriorityWin.BelowNormal:
        return 'Below normal';
      case ProcessPriorityWin.Idle:
        return 'Idle';
      default:
        return 'Unknown';
    }
  }

}

export interface ProcessDialogData {
  processInfo: ProcessInfo;
  onKill?: () => void;
}
