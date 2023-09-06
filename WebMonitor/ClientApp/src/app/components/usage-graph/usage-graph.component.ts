import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';

const verticalLinesSpace = 5;

@Component({
  selector: 'app-usage-graph',
  templateUrl: './usage-graph.component.html',
  styleUrls: ['./usage-graph.component.css']
})
export class UsageGraphComponent implements AfterViewInit {
  /**
   * Primary values for the graph
   */
  usages: number[] = [];
  /**
   * Secondary values for the graph
   */
  secondaryUsages: number[] = [];
  /**
   * Maximum amount of points to tract, discarding the oldest ones
   */
  @Input() maxPoints = 60;
  /**
   * Color of the graph
   */
  @Input() color = "black";
  /**
   * Color of the secondary line
   */
  @Input() secondaryColor = "red";
  /**
   * Whether the area under the graph should be filled
   */
  @Input() fill = true;
  /**
   * Whether the graph should be drawn with a grid
   */
  @Input() drawGrid = true;
  /**
   * Whether the maximum value should be relative to the maximum value of the usages
   */
  @Input() relativeMax = false;
  /**
   * Whether the secondary graph should be shown
   */
  @Input() showSecondary = false;

  @ViewChild("canvasElement")
  private canvasRef!: ElementRef<HTMLCanvasElement>;

  private gridOffset = 0;

  /**
   * Whether the graph should be refreshed
   */
  @Input() disableDraw = false;

  /**
   * Returns the current usage as a percentage relative to the maximum value
   */
  get currentUsage() {
    return (this.usages[this.usages.length - 1] ?? 0) / this.maxValue;
  }

  /**
   * Returns the secondary usage as a percentage relative to the maximum value
   */
  get currentSecondaryUsage() {
    return (this.secondaryUsages[this.secondaryUsages.length - 1] ?? 0) / this.maxValue;
  }

  private get maxValue() {
    if (this.relativeMax) {
      return Math.max(...this.usages, ...this.secondaryUsages);
    }

    return 100;
  }

  ngAfterViewInit(): void {
    // Remove extra points if they were passed
    while (this.usages.length > this.maxPoints) {
      this.usages.shift();
    }
    while (this.secondaryUsages.length > this.maxPoints) {
      this.secondaryUsages.shift();
    }
    // Draw starting points
    this.redrawGraph();
  }

  /**
   * Initializes the graph with the given values
   * @param usages Primary values
   * @param secondaryUsages Secondary values
   */
  initWithValues(usages: number[], secondaryUsages?: number[]) {
    this.usages = usages;
    if (secondaryUsages) {
      this.secondaryUsages = secondaryUsages;
    }

    this.ngAfterViewInit();
  }

  redrawGraph() {
    if (this.disableDraw) {
      return;
    }

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
    let w = canvas.width / (this.maxPoints - 2);
    ctx.lineWidth = window.devicePixelRatio;

    ctx.strokeStyle = this.color;
    // Graph is not transparent
    ctx.globalAlpha = 1;
    // Destination over ensures that the graph (which is drawn first) is always on top
    ctx.globalCompositeOperation = "destination-over";

    ctx.beginPath();

    // Get the starting offset
    let start = this.maxPoints - this.usages.length;
    // If there are more than maxPoints, start at 0
    if (start < 0)
      start = 0;

    // Top line is thicker
    ctx.lineWidth = window.devicePixelRatio * 2;
    // Draw the points
    ctx.moveTo(start * w, canvas.height - this.usages[0] / this.maxValue * canvas.height);
    for (let i = start; i < this.maxPoints; i++) {
      ctx.lineTo(i * w, canvas.height - this.usages[i - start] / this.maxValue * canvas.height);
    }
    ctx.stroke()

    if (this.fill) {
      // Fill is semi-transparent
      ctx.globalAlpha = 0.5;
      // Move to bottom right
      ctx.lineTo(canvas.width, canvas.height);
      // Move to starting x
      ctx.lineTo(start * w, canvas.height);
      ctx.closePath();
      // Fill the graph
      ctx.fillStyle = this.color;
      ctx.fill();
    }

    ctx.globalAlpha = 1;

    // Draw the secondary graph
    if (this.showSecondary) {
      ctx.strokeStyle = this.secondaryColor;
      ctx.lineWidth = window.devicePixelRatio * 2;
      ctx.beginPath();
      ctx.moveTo(start * w, canvas.height - this.secondaryUsages[0] / this.maxValue * canvas.height);
      for (let i = start; i < this.maxPoints; i++) {
        ctx.lineTo(i * w, canvas.height - this.secondaryUsages[i - start] / this.maxValue * canvas.height);
      }
      ctx.stroke()
    }
    // Reset line width
    ctx.lineWidth = window.devicePixelRatio;

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
      // Line 0 and 10 are the top and bottom borders of the graph so they are not drawn
      for (let i = 1; i < horizontalLines; i++) {
        ctx.moveTo(0, canvas.height / horizontalLines * i);
        ctx.lineTo(canvas.width, canvas.height / horizontalLines * i);
      }

      // Draw vertical lines
      for (let i = -this.gridOffset * w; i < canvas.width; i += w * verticalLinesSpace) {
        ctx.moveTo(i, 0);
        ctx.lineTo(i, canvas.height);
      }
      ctx.stroke();
    }
  }

  public addValue(value: number) {
    this.usages.push(value);
    if (this.usages.length > this.maxPoints) {
      this.usages.shift();
    }

    this.gridOffset = (this.gridOffset + 1) % verticalLinesSpace;

    this.redrawGraph();
  }

  public addValues(primary: number, secondary: number) {
    this.usages.push(primary);
    if (this.usages.length > this.maxPoints) {
      this.usages.shift();
    }

    this.secondaryUsages.push(secondary);
    if (this.secondaryUsages.length > this.maxPoints) {
      this.secondaryUsages.shift();
    }

    this.gridOffset = (this.gridOffset + 1) % verticalLinesSpace;

    this.redrawGraph();
  }

}
