import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ColorInputComponent } from './color-input.component';

describe('ColorInputComponent', () => {
  let component: ColorInputComponent;
  let fixture: ComponentFixture<ColorInputComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ColorInputComponent]
    });
    fixture = TestBed.createComponent(ColorInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
