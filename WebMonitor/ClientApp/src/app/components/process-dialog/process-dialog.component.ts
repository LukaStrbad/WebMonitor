import { Component, Inject, LOCALE_ID } from '@angular/core';
import { ProcessInfo } from "../../../model/process-info";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: 'app-process-dialog',
  templateUrl: './process-dialog.component.html',
  styleUrls: ['./process-dialog.component.css']
})
export class ProcessDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ProcessDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProcessDialogData,
    @Inject(LOCALE_ID) public locale: string
  ) {
  }

  onCloseClick() {
    this.dialogRef.close();
  }

}

export interface ProcessDialogData {
  processInfo: ProcessInfo;
  onKill?: () => void;
}
