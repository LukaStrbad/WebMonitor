import { Component, Inject, Input, LOCALE_ID } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FileInformation } from 'src/model/file-information';
import * as numberHelpers from "src/helpers/number-helpers";

@Component({
  selector: 'app-file-dialog',
  templateUrl: './file-dialog.component.html',
  styleUrls: ['./file-dialog.component.css']
})
export class FileDialogComponent {
  numberHelpers = numberHelpers;

  constructor(
    public dialogRef: MatDialogRef<FileDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FileDialogData,
    @Inject(LOCALE_ID) public locale: string
  ) { }

  onCloseClick() {
    this.dialogRef.close();
  }
}

export interface FileDialogData {
  fileInfo: FileInformation;
  download?: () => void;
}
