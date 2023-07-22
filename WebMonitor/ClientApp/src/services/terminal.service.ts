import { Inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { firstValueFrom } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class TerminalService {
  private readonly apiUrl: string;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "Terminal/";
  }

  async startNewSession() {
    return await firstValueFrom(this.http.get<number>(this.apiUrl + "startNewSession"));
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
