import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  ViewChild,
  ViewEncapsulation
} from '@angular/core';
import { Terminal } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';
import { WebLinksAddon } from 'xterm-addon-web-links';
import { AttachAddon } from 'xterm-addon-attach';
import { environment } from "../../environments/environment";
import { TerminalService } from "../../services/terminal.service";

@Component({
  selector: 'app-terminal',
  templateUrl: './terminal.component.html',
  styleUrls: ['./terminal.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class TerminalComponent implements AfterViewInit, OnDestroy {
  @ViewChild("terminal") terminal!: ElementRef<HTMLDivElement>;
  private term!: Terminal;
  private terminalSize = [80, 24];
  private letterSize: [number, number] | null = null;

  private socket!: WebSocket;
  private fitAddon!: FitAddon;

  constructor(private terminalService: TerminalService) {
  }

  ngAfterViewInit(): void {
    this.term = new Terminal({
      fontSize: 12,
      lineHeight: 1.2,
      fontFamily: 'Consolas, "Courier New", monospace',
      cursorBlink: true,
      scrollback: Number.MAX_SAFE_INTEGER,
    });
    this.fitAddon = new FitAddon();
    this.term.loadAddon(this.fitAddon);
    this.term.loadAddon(new WebLinksAddon());

    this.init().then(_ => {
    });
    this.term.resize(this.terminalSize[0], this.terminalSize[1]);
  }

  ngOnDestroy(): void {
    this.socket?.close();
  }

  async init() {
    const loc = environment.production ? location.host : "localhost:5158";
    const proto = location.protocol === 'https:' ? 'ws' : 'ws';

    let sessionId = this.terminalService.sessionId ?? await this.terminalService.startNewSession();

    const isAlive = await this.terminalService.isSessionAlive(sessionId);
    if (!isAlive) {
      this.term.write("\r\nSession has expired. Starting a new session...\r\n");
      sessionId = await this.terminalService.startNewSession();
    }

    this.socket = new WebSocket(`${proto}://${loc}/Terminal/session`);

    this.term.open(this.terminal.nativeElement);

    this.socket.onopen = (e) => {
      // Send the session ID to the server.
      this.socket.send(`${sessionId}\n`);

      const attachAddon = new AttachAddon(this.socket);
      this.term.loadAddon(attachAddon);

      if (this.term.element) {
        const top = this.term.element.getBoundingClientRect().top;
        const margin = "32px";
        this.term.element.style.height = `calc(100vh - ${top}px - ${margin})`;
      }

      // Resize on init
      const xtermRows = this.term.element?.querySelector(".xterm-rows");
      this.letterSize = [(xtermRows?.clientWidth ?? 0) / this.terminalSize[0], (xtermRows?.clientHeight ?? 0) / this.terminalSize[1]];
      this.onResize(null);
    }

    this.socket.onclose = event => {
      this.term.write("\r\nConnection closed.\r\n");
    };

    this.term.onResize(async (size) => {
      await this.terminalService.changePtySize(sessionId, size.cols, size.rows);
    });
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    const xterm = this.term.element;
    if (!xterm || !this.letterSize) {
      return;
    }

    const width = xterm.clientWidth;
    const height = xterm.clientHeight;
    const cols = Math.floor(width / this.letterSize[0]);
    const rows = Math.floor(height / this.letterSize[1]);

    // Only resize if the size has changed.
    if (cols === this.term.cols && rows === this.term.rows) {
      return;
    }

    this.term.resize(cols, rows);
    this.terminalSize = [cols, rows];
  }
}
