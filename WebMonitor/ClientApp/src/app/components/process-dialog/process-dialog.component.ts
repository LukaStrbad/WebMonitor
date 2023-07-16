import { AfterViewInit, Component, Inject, LOCALE_ID, OnDestroy } from '@angular/core';
import { ProcessInfo } from "../../../model/process-info";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { ExtendedProcessInfo } from "../../../model/extended-process-info";
import { SysInfoError, SysInfoService } from "../../../services/sys-info.service";
import { Subscription } from "rxjs";
import { ProcessPriorityWin } from "../../../model/process-priority";
import * as numberHelpers from "../../../helpers/number-helpers";
import { SupportedFeatures } from "../../../model/supported-features";
import { ManagerService } from "../../../services/manager.service";

@Component({
  selector: 'app-process-dialog',
  templateUrl: './process-dialog.component.html',
  styleUrls: ['./process-dialog.component.css']
})
export class ProcessDialogComponent implements OnDestroy, AfterViewInit {
  extendedInfo: ExtendedProcessInfo | null = null;
  affinityThreads: boolean[] | null = null;
  rowsArray: number[][] = [];
  subscription?: Subscription;
  error?: string;
  supportedFeatures?: SupportedFeatures;

  numberHelpers = numberHelpers;

  constructor(
    public dialogRef: MatDialogRef<ProcessDialogComponent>,
    private sysInfo: SysInfoService,
    private manager: ManagerService,
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

      this.affinityThreads = this.getAffinityArray(computerInfo.cpu.numThreads, info.affinity);
      this.rowsArray = this.getRowsArray(this.affinityThreads);
      console.log(this.rowsArray);
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

  private getRowsArray(affinityThreads: boolean[]): number[][] {
    const numberOfColumns = 4;
    const rows = Math.ceil(affinityThreads.length / numberOfColumns);
    let array: number[][] = [];

    for (let i = 0; i < rows; i++) {
      let row: number[] = [];
      for (let j = 0; j < numberOfColumns; j++) {
        row.push(i * numberOfColumns + j);
      }
      array.push(row);
    }

    return array;
  }

  /**
   * Converts a ProcessPriorityWin to a human readable string.
   * @param priority The ProcessPriorityWin to convert.
   */
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

  /**
   * Returns true if the thread is enabled.
   * @param threadCount The number of threads on system
   * @param affinity The affinity of the process
   */
  getAffinityArray(threadCount: number, affinity: bigint): boolean[] {
    const affinityArray: boolean[] = [];

    let binaryString = affinity.toString(2).padStart(threadCount, '0');

    for (let i = 0; i < threadCount; i++) {
      affinityArray.push(binaryString[threadCount - 1 - i] === '1');
    }

    return affinityArray;
  }

  /**
   * Whether all affinity threads are selected.
   */
  allAffinityThreadsSelected(): boolean {
    return this.affinityThreads?.every(x => x) ?? false;
  }

  /**
   * Toggle affinity for the given thread.
   * @param index
   */
  async changeAffinity(index: number) {
    const affinity = await this.manager.changeProcessAffinity({
      pid: this.data.processInfo.pid,
      threads: [{ threadIndex: index, on: !this.affinityThreads![index] }]
    });

    await this.updateAffinityThreads(affinity);
  }

  /**
   * Enable all threads.
   */
  async enableAllThreads() {
    const affinity = await this.manager.changeProcessAffinity({
      pid: this.data.processInfo.pid,
      threads: this.affinityThreads!.map((x, i) => ({ threadIndex: i, on: true }))
    });

    await this.updateAffinityThreads(affinity);
  }

  async updateAffinityThreads(affinity: bigint | null) {
    const computerInfo = await this.sysInfo.getComputerInfo();
    if (!computerInfo.cpu || !this.supportedFeatures?.processAffinity || !affinity) {
      return;
    }
    const affinityArray = this.getAffinityArray(computerInfo.cpu.numThreads, affinity);

    for (let i = 0; i < affinityArray.length; i++) {
      // Update only the values that have changed.
      if (this.affinityThreads![i] !== affinityArray[i]) {
        this.affinityThreads![i] = affinityArray[i];
      }
    }
  }
}

export interface ProcessDialogData {
  processInfo: ProcessInfo;
  onKill?: () => void;
}
