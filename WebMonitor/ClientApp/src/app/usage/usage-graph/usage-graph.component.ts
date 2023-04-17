import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'app-usage-graph',
  templateUrl: './usage-graph.component.html',
  styleUrls: ['./usage-graph.component.css']
})
export class UsageGraphComponent implements AfterViewInit {
  @Input() usages: number[] = [];
  @Input() maxPoints = 60;
  @Input() color = "black";
  @Input() fill = true;
  @Input() drawGrid = true;
  @Input() relativeMax = false;

  @ViewChild("canvasElement")
  canvasRef!: ElementRef<HTMLCanvasElement>;

  private gridOffset = 0;

  get currentUsage() {
    return (this.usages[this.usages.length - 1] ?? 0) / this.maxValue;
  }

  private get maxValue() {
    if (this.relativeMax) {
      return Math.max(...this.usages);
    }

    return 100;
  }

  ngAfterViewInit(): void {
    // Draw starting points
   this.redrawGraph();
  }

  private redrawGraph() {
    let canvas = this.canvasRef.nativeElement;

    // Resize the canvas so it doesn't look blurry
    canvas.width = canvas.clientWidth * window.devicePixelRatio;
    canvas.height = canvas.clientHeight * window.devicePixelRatio;

    let ctx = canvas.getContext('2d');
    if (ctx == null) {
      return;
    }

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Width between points
    let w = canvas.width / (this.maxPoints - 1);
    ctx.lineWidth = window.devicePixelRatio;

    ctx.strokeStyle = this.color;
    // Graph is not transparent
    ctx.globalAlpha = 1;
    // Destination over ensures that the graph (which is drawn first) is always on top
    ctx.globalCompositeOperation = "destination-over";

    ctx.beginPath();

    // Get the starting offset
    let start = this.maxPoints - this.usages.length;
    // If there are less than maxPoints, start at 0
    if (start < 0)
      start = 0;

    // Draw the points
    ctx.moveTo(start * w, canvas.height - this.usages[0] / this.maxValue * canvas.height);
    for (let i = start + 1; i < this.maxPoints; i++) {
      ctx.lineTo(i * w, canvas.height - this.usages[i - start] / this.maxValue * canvas.height);
    }

    if (this.fill) {
      // Move to bottom right
      ctx.lineTo(canvas.width, canvas.height);
      // Move to starting x
      ctx.lineTo(start * w, canvas.height);
      ctx.closePath();
      // Fill the graph
      ctx.fillStyle = this.color;
      ctx.fill();
    } else {
      ctx.stroke();
    }


    ctx.strokeStyle = "gray";
    // Border and grid are semi-transparent
    ctx.globalAlpha = 0.5;
    // Draw border
    ctx.strokeRect(0, 0, canvas.width, canvas.height);

    if (this.drawGrid) {
      // One line is equal to 10% of the graph
      const horizontalLines = 10;

      // Draw horizontal lines
      ctx.beginPath();
      for (let i = 0; i < horizontalLines; i++) {
        ctx.moveTo(0, canvas.height / horizontalLines * i);
        ctx.lineTo(canvas.width, canvas.height / horizontalLines * i);
      }

      // Calculate the number of vertical lines from width / height ratio
      // Floor ensures that the number of lines is always an integer
      const verticalLines = Math.floor(canvas.width / canvas.height * horizontalLines);

      // Calculate the actual offset
      const offset = this.gridOffset % verticalLines * w;

      // Draw vertical lines
      // TODO: Potentially optimize this by only drawing the lines that are visible
      for (let i = 0; i < verticalLines + 4; i++) {
        ctx.moveTo(canvas.width / verticalLines * i - offset, 0);
        ctx.lineTo(canvas.width / verticalLines * i - offset, canvas.height);
      }

      ctx.stroke();
    }
  }

  public addValue(value: number) {
    this.usages.push(value);
    if (this.usages.length > this.maxPoints) {
      this.usages.shift();
    }

    this.gridOffset = (this.gridOffset + 1) % this.maxPoints;

    this.redrawGraph();
  }

}
