import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PointOfInterestMapComponent } from './point-of-interest-map.component';

describe('PointOfInterestMapComponent', () => {
  let component: PointOfInterestMapComponent;
  let fixture: ComponentFixture<PointOfInterestMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PointOfInterestMapComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PointOfInterestMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
