import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupportedFeaturesCard } from './supported-features-card.component';

describe('FeaturesCardComponent', () => {
  let component: SupportedFeaturesCard;
  let fixture: ComponentFixture<SupportedFeaturesCard>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SupportedFeaturesCard]
    });
    fixture = TestBed.createComponent(SupportedFeaturesCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
