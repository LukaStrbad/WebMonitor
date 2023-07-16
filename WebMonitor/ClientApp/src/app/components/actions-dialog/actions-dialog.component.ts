import { Component, Inject, LOCALE_ID } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: 'app-actions-dialog',
  templateUrl: './actions-dialog.component.html',
  styleUrls: ['./actions-dialog.component.css']
})
export class ActionsDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: ActionsDialogData) {
  }
}

export interface ActionsDialogData {
  title: string;
  content: string;
  positiveButton?: [string, () => void];
}
