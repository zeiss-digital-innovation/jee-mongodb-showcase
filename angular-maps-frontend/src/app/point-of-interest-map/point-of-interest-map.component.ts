import { Component, OnInit } from '@angular/core';
import { PointOfInterest } from '../model/point_of_interest';
import { PointOfInterestService } from '../service/point-of-interest.service';
import * as L from 'leaflet';
import { MapDataService } from '../service/map-data.service';
import { CommonModule } from '@angular/common';
import { ApplicationRef, createComponent, EnvironmentInjector } from '@angular/core';
import { POI_CATEGORIES } from '../model/poi-categories';
import { PoiDialogComponent } from '../poi-dialog/poi-dialog.component';

// Fix for default markers in Leaflet with Angular
const iconRetinaUrl = 'media/leaflet/marker-icon-2x.png';
const iconUrl = 'media/leaflet/marker-icon.png';
const shadowUrl = 'media/leaflet/marker-shadow.png';
const iconDefault = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

@Component({
  selector: 'app-point-of-interest-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './point-of-interest-map.component.html',
  styleUrl: './point-of-interest-map.component.css'
})
export class PointOfInterestMapComponent implements OnInit {

  pointsOfInterest: PointOfInterest[] = [];
  map: L.Map | undefined;

  latitudeDefault = 51.0504;
  longitudeDefault = 13.7373;
  zoomDefault = 13;

  constructor(private poiService: PointOfInterestService, private mapDataService: MapDataService,
    private appRef: ApplicationRef, private injector: EnvironmentInjector) {
  }

  ngOnInit(): void {
    // Initialize the map
    this.map = L.map('map').setView([this.latitudeDefault, this.longitudeDefault], this.zoomDefault);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(this.map);

    this.loadPointsOfInterest(this.latitudeDefault, this.longitudeDefault, this.mapDataService.getRadiusForZoom(this.zoomDefault));

    this.map.on('moveend', () => {
      const center = this.map!.getCenter();
      const newLatitude = center.lat;
      const newLongitude = center.lng;
      const newRadius = this.mapDataService.getRadiusForZoom(this.map!.getZoom());
      this.loadPointsOfInterest(newLatitude, newLongitude, newRadius);
    });

    // use Leaflet's contextmenu event for right-clicks (reliable place to prevent the browser context menu)
    this.map.on('contextmenu', (event) => {
      // stop the browser context menu - prefer the contextmenu event to preventDefault on the correct event
      try {
        if (event.originalEvent && typeof event.originalEvent.preventDefault === 'function') {
          event.originalEvent.preventDefault();
        }
      } catch (e) {
        // ignore if not supported
      }

      // event.latlng is provided by Leaflet on contextmenu
      const coords = (event as any).latlng || this.map!.mouseEventToLatLng((event as any).originalEvent);
      // enable for debugging
      //console.log('Right-click (contextmenu) at:', coords);
      this.addMarkerAt(coords.lat, coords.lng);
    });

    // defensive: also prevent native context menu on the map container element
    try {
      const container = this.map.getContainer();
      container.addEventListener('contextmenu', (e) => e.preventDefault());
    } catch (e) {
      // ignore if not available
    }
  }

  addMarkerAt(latitude: number, longitude: number): void {
    if (!this.map) {
      console.error('Map is not initialized');
      return;
    }
    // create and attach the Angular dialog component to the document
    const compRef = createComponent(PoiDialogComponent, { environmentInjector: this.injector });
    // set inputs before attaching the view so CD picks them up
    compRef.instance.latitude = latitude;
    compRef.instance.longitude = longitude;
    compRef.instance.categories = POI_CATEGORIES as unknown as string[];

    // attach the component view to the application so change detection runs
    this.appRef.attachView(compRef.hostView);
    compRef.changeDetectorRef.detectChanges();

    const el = compRef.location.nativeElement as HTMLElement;
    // attach to body so that CSS position:fixed backdrop works
    document.body.appendChild(el);

    const cleanupComponent = () => {
      try {
        if (el && el.parentNode) el.parentNode.removeChild(el);
      } catch (e) {
        // ignore
      }
      try {
        this.appRef.detachView(compRef.hostView);
      } catch (e) {
        // ignore
      }
      try {
        compRef.destroy();
      } catch (e) {
        // ignore
      }
    };

    compRef.instance.cancel.subscribe(() => {
      cleanupComponent();
    });

    compRef.instance.save.subscribe(({ category, details }) => {
      const poi: PointOfInterest = {
        href: '',
        category: category,
        details: details,
        location: {
          coordinates: [longitude, latitude],
          type: 'Point'
        }
      };

      this.poiService.createPointOfInterest(poi).subscribe({
        next: (created) => {
          const displayPoi = created ?? poi;
          let popupContent = '';
          try {
            popupContent = this.mapDataService.getMarkerPopupFor(displayPoi);
          } catch (e) {
            console.warn('getMarkerPopupFor failed, falling back to plain details', e);
            popupContent = displayPoi.details || '';
          }
          L.marker([latitude, longitude]).addTo(this.map!)
            .bindPopup(popupContent);
          this.pointsOfInterest.push(displayPoi);
          cleanupComponent();
        },
        error: (err) => {
          console.error('Failed to create POI', err);
          alert('Failed to create POI');
          cleanupComponent();
        }
      });
    });

    // attach to body so that CSS position:fixed backdrop works
    document.body.appendChild((compRef.location.nativeElement as HTMLElement));
  }


  loadPointsOfInterest(latitude: number, longitude: number, radius: number): void {
    this.poiService.getPointsOfInterest(latitude, longitude, radius)
      .subscribe(points => {
        this.showPointsOnMap(points);
      });
  }

  showPointsOnMap(points: PointOfInterest[]): void {
    this.pointsOfInterest = points;

    if (!this.map) {
      console.error('Map is not initialized');
      return;
    }

    // remove all current markers
    this.map.eachLayer((layer) => {
      if (layer instanceof L.Marker) {
        this.map!.removeLayer(layer);
      }
    });

    this.pointsOfInterest.forEach(poi => {
      const coords = poi.location.coordinates;

      L.marker([coords[1], coords[0]]).addTo(this.map!)
        .bindPopup(this.mapDataService.getMarkerPopupFor(poi));
    });
  }

  onCategoryChange($event: Event) {
    const selectedCategory = ($event.target as HTMLSelectElement).value;
    // TODO filter by Category
    //this.filterPointsOfInterestByCategory(selectedCategory);
  }
}
