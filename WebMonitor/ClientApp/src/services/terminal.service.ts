import { Inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { firstValueFrom } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class TerminalService {
  private readonly apiUrl: string;
  sessionId: number | null = null;

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
