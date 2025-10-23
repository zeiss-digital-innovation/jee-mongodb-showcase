import { AfterViewInit, ApplicationRef, Component, createComponent, ElementRef, EnvironmentInjector, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

import { environment } from '../environments/environment';

import { PointOfInterest } from '../model/point_of_interest';
import { POI_CATEGORIES } from '../model/poi-categories';
import { ToastNotification } from '../model/toast_notification';

import { MapDataService } from '../service/map-data.service';
import { PointOfInterestService } from '../service/point-of-interest.service';
import { PoiFilterService } from '../service/poi-filter.service';
import { SearchCriteriaService } from '../service/search-criteria-service';

import { PoiDialogComponent } from '../poi-dialog/poi-dialog.component';

import * as L from 'leaflet';

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
export class PointOfInterestMapComponent implements OnInit, AfterViewInit {

  categories = POI_CATEGORIES;

  categoryFilter: string | undefined;
  detailsFilter: string | undefined;

  pointsOfInterest: PointOfInterest[] = [];
  pointsOfInterestFiltered: PointOfInterest[] = [];

  map: L.Map | undefined;

  zoomDefault: number;

  latitude: number;
  longitude: number;
  radius: number;

  durationOfRequest: number = 0;

  toastNotification: ToastNotification = new ToastNotification(ToastNotification.titleDefault, '', '', '');

  @ViewChild('messageToast', { static: false }) messageToastRef!: ElementRef<HTMLElement>;
  private messageToastInstance?: any;

  constructor(private poiService: PointOfInterestService, public poiFilterService: PoiFilterService,
    private searchCriteriaService: SearchCriteriaService, private mapDataService: MapDataService,
    private appRef: ApplicationRef, private injector: EnvironmentInjector) {

    this.latitude = environment.latitudeDefault;
    this.longitude = environment.longitudeDefault;
    this.radius = environment.radiusDefault;
    this.zoomDefault = environment.zoomDefault;
  }

  ngOnInit(): void {
    const filterCriteria = this.poiFilterService.getFilterCriteria();

    if (filterCriteria) {
      this.categoryFilter = filterCriteria.categoryFilter;
      this.detailsFilter = filterCriteria.detailsFilter;
    }

    const searchData = this.searchCriteriaService.getSearchCriteria();

    if (searchData) {
      this.latitude = searchData.latitude;
      this.longitude = searchData.longitude;
      this.radius = searchData.radius;
    }

    // Initialize the map
    this.map = L.map('map').setView([this.latitude, this.longitude], this.zoomDefault);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(this.map);

    this.loadPointsOfInterest(this.latitude, this.longitude, this.mapDataService.getRadiusForZoom(this.zoomDefault));

    this.map.on('moveend', () => {
      const center = this.map!.getCenter();
      this.latitude = center.lat;
      this.longitude = center.lng;
      this.radius = this.mapDataService.getRadiusForZoom(this.map!.getZoom());
      this.loadPointsOfInterest(this.latitude, this.longitude, this.radius);
      this.searchCriteriaService.setSearchCriteria({ latitude: this.latitude, longitude: this.longitude, radius: this.radius });
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

  ngAfterViewInit(): void {
    // instantiate the Toast via dynamic import to avoid TypeScript module resolution issues
    // If bootstrap is included globally (via angular.json scripts) the fallback will use window.bootstrap
    import('bootstrap/js/dist/toast')
      .then(mod => {
        const ToastClass = (mod && (mod as any).default) ? (mod as any).default : (mod as any);
        this.messageToastInstance = new ToastClass(this.messageToastRef.nativeElement);
      })
      .catch(() => {
        const G = (window as any).bootstrap;
        if (G && G.Toast) {
          this.messageToastInstance = new G.Toast(this.messageToastRef.nativeElement);
        } else {
          // as last resort, no Toast available
          console.warn('Bootstrap Toast not available');
        }
      });
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

      const startTime = performance.now();

      this.poiService.createPointOfInterest(poi).subscribe({
        next: (created) => {
          this.durationOfRequest = performance.now() - startTime;

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

          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully added new point of interest',//
            this.durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);

          cleanupComponent();
        },
        error: (err) => {
          console.error('Failed to create POI', err);
          this.showToastMessage(ToastNotification.titleDefault, 'Failed to create POI. Please try again later.', '', ToastNotification.cssClassError);
          cleanupComponent();
        }
      });
    });

    // attach to body so that CSS position:fixed backdrop works
    document.body.appendChild((compRef.location.nativeElement as HTMLElement));
  }

  loadPointsOfInterest(latitude: number, longitude: number, radius: number): void {
    // determine the time duration of the request
    const startTime = performance.now();

    this.poiService.getPointsOfInterest(latitude, longitude, radius)
      .subscribe({
        next: points => {
          this.pointsOfInterest = points;
          this.pointsOfInterestFiltered = this.poiFilterService.filter(this.pointsOfInterest, this.categoryFilter, this.detailsFilter);
          this.updateFiltering();
          this.showPointsOnMap();

          this.durationOfRequest = performance.now() - startTime;
          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully loaded ' + this.pointsOfInterest.length + ' points of interest',//
            this.durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);
        },
        error: err => {
          console.error('Failed to load POIs', err);
          this.showToastMessage(ToastNotification.titleDefault, 'POI Service is currently not available. Please try again later.', '', ToastNotification.cssClassError);
        }
      });
  }

  showPointsOnMap(): void {
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

    this.pointsOfInterestFiltered.forEach(poi => {
      const coords = poi.location.coordinates;

      L.marker([coords[1], coords[0]]).addTo(this.map!)
        .bindPopup(this.mapDataService.getMarkerPopupFor(poi));
    });
  }

  filterBySearch(event: Event) {
    const search = (event.target as HTMLInputElement).value;

    this.detailsFilter = search;
    this.updateFiltering();
  }

  filterByCategory(event: Event) {
    const category = (event.target as HTMLInputElement).value;

    this.categoryFilter = category;

    if (!category || category === 'Choose...') {
      this.categoryFilter = undefined;
    }

    this.updateFiltering();
  }

  showToastMessage(title: string, message: string, smallMessage: string, cssClass: string, attempt = 0) {
    if (this.messageToastInstance) {
      this.toastNotification = new ToastNotification(title, message, smallMessage, cssClass);
      this.messageToastInstance.show();
      return;
    }
    if (attempt < ToastNotification.retryCount) {
      setTimeout(() => this.showToastMessage(title, message, smallMessage, cssClass, attempt + 1), ToastNotification.retryDelay);
    } else {
      console.warn('Success toast not available to show');
    }
  }

  private updateFiltering() {
    this.poiFilterService.setFilterCriteria({ detailsFilter: this.detailsFilter, categoryFilter: this.categoryFilter });
    this.pointsOfInterestFiltered = this.poiFilterService.filter(this.pointsOfInterest, this.categoryFilter, this.detailsFilter);
    this.showPointsOnMap();
  }
}
