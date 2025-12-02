import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActionsDialogComponent } from './actions-dialog.component';

describe('ActionsDialogComponent', () => {
  let component: ActionsDialogComponent;
  let fixture: ComponentFixture<ActionsDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ActionsDialogComponent]
    });
    fixture = TestBed.createComponent(ActionsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
