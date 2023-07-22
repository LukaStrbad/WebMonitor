import { EventEmitter, Inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { catchError, firstValueFrom, throwError } from "rxjs";
import { ChangePriorityRequest } from "../model/requests/change-priority-request";
import { ProcessPriorityWin } from "../model/process-priority";
import { ChangeAffinityRequest } from "../model/requests/change-affinity-request";

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

  async changeProcessPriority(value: ChangePriorityRequest) {
    const response = await firstValueFrom(
      this.http.post<ProcessPriorityWin | number>(this.apiUrl + "changeProcessPriority", value, { observe: "response" })
        .pipe(catchError(e => this.handleError(e, `Failed to change priority of process with PID ${value.pid}`)))
    );

    if (response.ok && (value.priorityWin === response.body || value.priorityLinux === response.body)) {
      this.okEmitter.emit(`Successfully changed priority of process with PID ${value.pid}`);
    }

    return response.body;
  }

  async changeProcessAffinity(value: ChangeAffinityRequest) {
    const response = await firstValueFrom(
      this.http.post<bigint>(this.apiUrl + "changeProcessAffinity", value, { observe: "response" })
        .pipe(catchError(e => this.handleError(e, `Failed to change affinity of process with PID ${value.pid}`)))
    );

    if (response.ok) {
      this.okEmitter.emit(`Successfully changed affinity of process with PID ${value.pid}`);
    }

    return response.body;
  }
}
