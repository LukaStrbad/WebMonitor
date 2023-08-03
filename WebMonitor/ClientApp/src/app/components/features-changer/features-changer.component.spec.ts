import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeaturesChangerComponent } from './features-changer.component';

describe('FeaturesChangerComponent', () => {
  let component: FeaturesChangerComponent;
  let fixture: ComponentFixture<FeaturesChangerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [FeaturesChangerComponent]
    });
    fixture = TestBed.createComponent(FeaturesChangerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
