import { Component, Inject, LOCALE_ID } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: 'app-actions-dialog',
  templateUrl: './actions-dialog.component.html',
  styleUrls: ['./actions-dialog.component.css']
})
export class ActionsDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: ActionsDialogData) {
    // If the negative button is not set, set it to a default value
    if (!data.negativeButton) {
      data.negativeButton = ["Close", () => {}];
    }
  }
}

export interface ActionsDialogData {
  title: string;
  content: string;
  positiveButton?: [string, () => void];
  negativeButton?: [string, () => void];
}
