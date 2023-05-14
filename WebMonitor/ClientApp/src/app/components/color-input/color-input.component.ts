import { EventEmitter, Output } from '@angular/core';
import { AfterViewInit, Component, ElementRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'app-color-input',
  templateUrl: './color-input.component.html',
  styleUrls: ['./color-input.component.css']
})
export class ColorInputComponent implements AfterViewInit {
  @Input() label = "";
  @Input() color = "";
  @Output() colorChange = new EventEmitter<string>();

  @ViewChild("colorIcon", { read: ElementRef })
  private colorIcon!: ElementRef<HTMLElement>;

  // ViewChild is only available after the view has been initialized
  ngAfterViewInit(): void {
    this.changeIconColor();
  }

  /**
   * Event handler for when the color input is changed
   * @param val The event that triggered the change
   */
  onKeyUp(val: KeyboardEvent) {
    const input = val.target as HTMLInputElement;
    this.color = input.value;
    this.changeIconColor();
  }

  /**
   * Changes the color of the icon to the current color
   */
  private changeIconColor() {
    const icon = this.colorIcon.nativeElement;
    icon.style.color = this.color;

    // Empty string signifies invalid color
    if (icon.style.color !== "") {
      this.colorChange.emit(this.color);
    }
  }
}
