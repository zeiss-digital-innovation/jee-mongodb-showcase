import { AfterViewInit, ApplicationRef, Component, createComponent, ElementRef, EnvironmentInjector, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { environment } from '../environments/environment';

import { PointOfInterest } from '../model/point_of_interest';
import { POI_CATEGORIES } from '../model/poi-categories';
import { ToastNotification } from '../model/toast_notification';

import { PointOfInterestService } from '../service/point-of-interest.service';
import { PoiFilterService } from '../service/poi-filter.service';
import { SearchCriteriaService } from '../service/search-criteria-service';

import { FormatDetailsPipe } from '../pipe/format-details-pipe';
import { PoiDialogComponent } from '../poi-dialog/poi-dialog.component';

@Component({
  selector: 'app-point-of-interest-list',
  standalone: true,
  imports: [CommonModule, FormsModule, FormatDetailsPipe],
  templateUrl: './point-of-interest-list.component.html',
  styleUrl: './point-of-interest-list.component.css'
})
export class PointOfInterestListComponent implements OnInit, AfterViewInit {

  latitude: number;
  longitude: number;
  radius: number;

  categories = POI_CATEGORIES;

  categoryFilter: string | undefined;
  detailsFilter: string | undefined;

  pointsOfInterest: PointOfInterest[] = [];
  pointsOfInterestFiltered: PointOfInterest[] = [];

  toastNotification: ToastNotification = new ToastNotification(ToastNotification.titleDefault, '', '', '');

  @ViewChild('messageToast', { static: false }) messageToastRef!: ElementRef<HTMLElement>;
  private messageToastInstance?: any;

  constructor(private poiService: PointOfInterestService, public poiFilterService: PoiFilterService,
    private searchCriteriaService: SearchCriteriaService,
    private appRef: ApplicationRef, private injector: EnvironmentInjector) {

    this.latitude = environment.latitudeDefault;
    this.longitude = environment.longitudeDefault;
    this.radius = environment.radiusDefault;
  }

  ngOnInit(): void {
    const filterCriteria = this.poiFilterService.getFilterCriteria();

    if (filterCriteria) {
      this.categoryFilter = filterCriteria.categoryFilter;
      console.log('Category filter on init:', this.categoryFilter);
      this.detailsFilter = filterCriteria.detailsFilter;
    }

    const searchData = this.searchCriteriaService.getSearchCriteria();

    if (searchData) {
      this.latitude = searchData.latitude;
      this.longitude = searchData.longitude;
      this.radius = searchData.radius;
    }

    this.poiService.getPointsOfInterest(this.latitude, this.longitude, this.radius)
      .subscribe(points => {
        this.pointsOfInterest = points;
        this.pointsOfInterestFiltered = this.pointsOfInterest;
        this.updateFiltering();
      });
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

  setRadius(event: Event): void {
    this.radius = Number((event.target as HTMLInputElement).value);
  }

  loadPointsOfInterest(): void {
    this.searchCriteriaService.setSearchCriteria({ latitude: this.latitude, longitude: this.longitude, radius: this.radius });

    // determine the time duration of the request
    const startTime = performance.now();

    this.poiService.getPointsOfInterest(this.latitude, this.longitude, this.radius)
      .subscribe({
        next: (points) => {
          this.pointsOfInterest = points;

          const durationOfRequest = performance.now() - startTime;
          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully loaded ' + this.pointsOfInterest.length + ' points of interest',//
            durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);

          this.pointsOfInterestFiltered = this.pointsOfInterest;
          this.updateFiltering();
        },
        error: err => {
          console.error('Failed to load POIs', err);
          this.showToastMessage(ToastNotification.titleDefault, 'POI Service is currently not available. Please try again later.', '', ToastNotification.cssClassError);
        }
      });

  }

  get coordsValid(): boolean {
    if (this.latitude < -90 || this.latitude > 90) return false;
    if (this.longitude < -180 || this.longitude > 180) return false;
    return true;
  }

  editPoi(point: PointOfInterest): void {
    // create and attach the Angular dialog component to the document
    const compRef = createComponent(PoiDialogComponent, { environmentInjector: this.injector });
    // set inputs before attaching the view so CD picks them up
    compRef.instance.details = point.details;
    compRef.instance.categories = POI_CATEGORIES as unknown as string[];
    compRef.instance.category = point.category;

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
      console.log(`Saving changes to POI ${point.href}: category=${category}, details=${details}`);
      point.category = category;
      point.details = details;

      // determine the time duration of the request
      const startTime = performance.now();

      this.poiService.updatePointOfInterest(point).subscribe({
        next: (updated) => {
          const durationOfRequest = performance.now() - startTime;
          console.log('POI updated:', updated);

          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully updated point of interest',//
            durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);
        },
        error: (err) => {
          console.error('Error updating POI:', err);
          this.showToastMessage(ToastNotification.titleDefault, 'Failed to update the point of interest. Please try again later.', '', ToastNotification.cssClassError);
        }
      });

      cleanupComponent();
    });
  }

  deletePoi(point: PointOfInterest): void {
    if (confirm('Are you sure you want to delete this point of interest?\n' + point.details)) {
      // determine the time duration of the request
      const startTime = performance.now();

      this.poiService.deletePointOfInterest(point).subscribe({
        next: () => {
          const durationOfRequest = performance.now() - startTime;

          // Remove the deleted point from the local array
          this.pointsOfInterest = this.pointsOfInterest.filter(p => p !== point);
          this.pointsOfInterestFiltered = this.pointsOfInterest;

          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully deleted point of interest',//
            durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);
        },
        error: (err) => {
          console.error('Error deleting point of interest:', err);
          this.showToastMessage(ToastNotification.titleDefault, 'Failed to delete the point of interest. Please try again later.', '', ToastNotification.cssClassError);
        }
      });
    }
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
  }

}
