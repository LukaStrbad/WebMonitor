import { AfterViewInit, Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { Terminal } from 'xterm';
import { FitAddon } from 'xterm-addon-fit';
import { WebLinksAddon } from 'xterm-addon-web-links';
import { AttachAddon } from 'xterm-addon-attach';
import { environment } from "../../environments/environment";

@Component({
  selector: 'app-terminal',
  templateUrl: './terminal.component.html',
  styleUrls: ['./terminal.component.css']
})
export class TerminalComponent implements AfterViewInit {
  @ViewChild("terminal") terminal!: ElementRef<HTMLDivElement>;
  private term!: Terminal;

  CSI = '\x1B[';
  private socket!: WebSocket;

  ngAfterViewInit(): void {
    this.term = new Terminal({
      fontSize: 12,
      lineHeight: 1.2,
      fontFamily: 'Consolas, "Courier New", monospace',
      cursorBlink: true,
      scrollback: Number.MAX_SAFE_INTEGER,
    });
    this.term.loadAddon(new FitAddon());
    this.term.loadAddon(new WebLinksAddon());

    const loc = environment.production ? location.host : "localhost:5158";
    const proto = location.protocol === 'https:' ? 'ws' : 'ws';
    this.socket = new WebSocket(`${proto}://${loc}/terminal`);
    this.term.open(this.terminal.nativeElement);

    this.socket.onopen = (e) => {
      const attachAddon = new AttachAddon(this.socket);
      this.term.loadAddon(attachAddon);
    }

    this.socket.onclose = event => {
      this.term.write("\r\nConnection closed.\r\n");
    };
  }
}
