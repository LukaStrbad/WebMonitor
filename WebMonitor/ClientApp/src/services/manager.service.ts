import { EventEmitter, Inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { catchError, firstValueFrom, throwError } from "rxjs";
import { showErrorSnackbar } from "../helpers/snackbar-helpers";

@Injectable({
  providedIn: 'root'
})
export class ManagerService {
  private readonly apiUrl: string;
  okEmitter = new EventEmitter<string>();
  errorEmitter = new EventEmitter<string>();

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "Manager/";
  }

  async killProcess(pid: number) {
    const response = await firstValueFrom(
      this.http.post<HttpResponse<string>>(this.apiUrl + "killProcess", pid, { observe: "response" })
        .pipe(catchError(e => this.handleError(e, `Failed to kill process with PID ${pid}`)))
    );

    if (response.ok) {
      this.okEmitter.emit(`Successfully killed process "${response.body}"`);
    }
  }

  private handleError(error: HttpErrorResponse, msg?: string) {
    if (msg) {
      this.errorEmitter.emit(msg);
    } else {
      this.errorEmitter.emit("Error: " + error.error);
    }
    return throwError(() => error.error);
  }
}
