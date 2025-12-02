import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileDialogComponent } from './file-dialog.component';

describe('FileDialogComponent', () => {
  let component: FileDialogComponent;
  let fixture: ComponentFixture<FileDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [FileDialogComponent]
    });
    fixture = TestBed.createComponent(FileDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
