import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PointOfInterestMapComponent } from './point-of-interest-map.component';

describe('PointOfInterestMapComponent', () => {
  let component: PointOfInterestMapComponent;
  let fixture: ComponentFixture<PointOfInterestMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PointOfInterestMapComponent, HttpClientTestingModule]
    })
      .compileComponents();

    fixture = TestBed.createComponent(PointOfInterestMapComponent);
    component = fixture.componentInstance;

    // create a dummy map container so Leaflet initialization in ngOnInit doesn't fail
    const mapEl = document.createElement('div');
    mapEl.id = 'map';
    mapEl.style.width = '600px';
    mapEl.style.height = '400px';
    document.body.appendChild(mapEl);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  afterEach(() => {
    const el = document.getElementById('map');
    if (el && el.parentNode) el.parentNode.removeChild(el);
  });
});
