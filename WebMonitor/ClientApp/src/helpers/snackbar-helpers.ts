import { MatSnackBar } from "@angular/material/snack-bar";

export function showOkSnackbar(snackBar: MatSnackBar, message: string) {
  snackBar.open(message, undefined, {
    horizontalPosition: "left",
    verticalPosition: "bottom",
    duration: 2000
  });
}

export function showErrorSnackbar(snackBar: MatSnackBar, message: string) {
  snackBar.open(message, undefined, {
    horizontalPosition: "left",
    verticalPosition: "bottom",
    duration: 3000,
    panelClass: ["error-snackbar"]
  });
}
