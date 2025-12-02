import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProcessDialogComponent } from './process-dialog.component';

describe('ProcessDialogComponent', () => {
  let component: ProcessDialogComponent;
  let fixture: ComponentFixture<ProcessDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ProcessDialogComponent]
    });
    fixture = TestBed.createComponent(ProcessDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
