import { Inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { firstValueFrom } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class TerminalService {
  private readonly apiUrl: string;

  get sessionId(): number | null {
    const sessionId = sessionStorage.getItem("terminalSessionId");
    if (sessionId === null) {
      return null;
    }
    const parsed = parseFloat(sessionId);
    // Return null if the value is not a number
    if (isNaN(parsed)) {
      return null;
    }
    return parsed;
  }

  set sessionId(value: number | null) {
    if (value === null) {
      sessionStorage.removeItem("terminalSessionId");
    } else {
      sessionStorage.setItem("terminalSessionId", value.toString());
    }
  }


  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "Terminal/";
  }

  async isSessionAlive(sessionId: number) {
    return await firstValueFrom(this.http.get<boolean>(this.apiUrl + "isSessionAlive?sessionId=" + sessionId));
  }

  async startNewSession() {
    this.sessionId = await firstValueFrom(this.http.get<number>(this.apiUrl + "startNewSession"));
    return this.sessionId;
  }

  async changePtySize(sessionId: number, cols: number, rows: number) {
    await firstValueFrom(this.http.post(this.apiUrl + "changePtySize",
      {
        sessionId,
        cols,
        rows
      })
    );
  }
}
