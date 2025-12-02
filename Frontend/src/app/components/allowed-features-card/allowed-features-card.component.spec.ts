import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllowedFeaturesCardComponent } from './allowed-features-card.component';

describe('AllowedFeaturesCardComponent', () => {
  let component: AllowedFeaturesCardComponent;
  let fixture: ComponentFixture<AllowedFeaturesCardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AllowedFeaturesCardComponent]
    });
    fixture = TestBed.createComponent(AllowedFeaturesCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
