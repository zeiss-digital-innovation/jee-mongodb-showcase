import { AfterViewInit, ApplicationRef, ChangeDetectorRef, Component, createComponent, ElementRef, EnvironmentInjector, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { environment } from '../environments/environment';

import { PointOfInterest } from '../model/point_of_interest';
import { POI_CATEGORIES, getBootstrapIconClass } from '../model/poi-categories';
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

  nameFilter: string | undefined;
  categoryFilter: string | undefined;
  detailsFilter: string | undefined;

  nameSort: boolean = false;
  categorySort: boolean = false;
  detailsSort: boolean = false;
  sortOrder: 'asc' | 'desc' = 'asc';

  pointsOfInterest: PointOfInterest[] = [];
  pointsOfInterestFiltered: PointOfInterest[] = [];

  selectedPoi?: PointOfInterest;

  toastNotification: ToastNotification = new ToastNotification(ToastNotification.titleDefault, '', '', '');

  @ViewChild('messageToast', { static: false }) messageToastRef!: ElementRef<HTMLElement>;
  private messageToastInstance?: any;
  private currentBsModal?: any;

  constructor(private poiService: PointOfInterestService, public poiFilterService: PoiFilterService,
    private searchCriteriaService: SearchCriteriaService,
    private appRef: ApplicationRef, private injector: EnvironmentInjector,
    private cd: ChangeDetectorRef) {

    this.latitude = environment.latitudeDefault;
    this.longitude = environment.longitudeDefault;
    this.radius = environment.radiusDefault;
  }

  ngOnInit(): void {
    const filterCriteria = this.poiFilterService.getFilterCriteria();

    if (filterCriteria) {
      console.log('Category filter on init:', this.categoryFilter);
      this.categoryFilter = filterCriteria.categoryFilter;
      this.nameFilter = filterCriteria.nameFilter;
      this.detailsFilter = filterCriteria.detailsFilter;
    }

    const searchData = this.searchCriteriaService.getSearchCriteria();

    if (searchData) {
      this.latitude = searchData.latitude;
      this.longitude = searchData.longitude;
      this.radius = searchData.radius;
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

    this.loadPointsOfInterest();
  }

  setRadius(event: Event): void {
    this.radius = Number((event.target as HTMLInputElement).value);
  }

  loadPointsOfInterest(): void {
    this.searchCriteriaService.setSearchCriteria({ latitude: this.latitude, longitude: this.longitude, radius: this.radius });
    console.log('Loading POIs for lat=' + this.latitude + ', lon=' + this.longitude + ', radius=' + this.radius);
    // determine the time duration of the request
    const startTime = performance.now();

    this.poiService.getPointsOfInterest(this.latitude, this.longitude, this.radius)
      .subscribe({
        next: (points) => {
          this.pointsOfInterest = points;

          const durationOfRequest = performance.now() - startTime;

          this.pointsOfInterestFiltered = this.pointsOfInterest;
          this.updateFiltering();

          this.showToastMessage(ToastNotification.titleDefault, //
            'Successfully loaded ' + this.pointsOfInterest.length + ' point(s) of interest. ' + this.pointsOfInterestFiltered.length + ' point(s) match the current filters.',//
            durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);
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
    compRef.instance.categories = POI_CATEGORIES as unknown as string[];
    compRef.instance.pointOfInterest = point;
    compRef.instance.action = 'Edit';
    compRef.instance.cssClass = 'bi bi-pencil';

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

    compRef.instance.save.subscribe(({ pointOfInterest }) => {
      console.log(`Saving changes to POI ${pointOfInterest?.href}: category=${pointOfInterest?.category}, name=${pointOfInterest?.name}, details=${pointOfInterest?.details}`);

      // determine the time duration of the request
      const startTime = performance.now();

      if (pointOfInterest) {
        this.poiService.updatePointOfInterest(pointOfInterest).subscribe({
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
      }

      cleanupComponent();
    });
  }

  filterByName(event: Event) {
    const search = (event.target as HTMLInputElement).value;

    this.nameFilter = search;
    this.updateFiltering();
  }

  filterByDetails(event: Event) {
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

  getBootstrapIconClass(category: string | undefined): string {
    return getBootstrapIconClass(category);
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

  /**
   * Sorts the filtered POI list by name. If already sorted by name, toggles the sort order.
   * @param keepSortOrder If true, keeps the current sort order instead of toggling it.
   */
  sortByName(keepSortOrder: boolean = false) {
    this.categorySort = false;
    this.detailsSort = false;

    if (!this.nameSort) {
      this.nameSort = true;
      this.sortOrder = 'asc';
    } else {
      if (!keepSortOrder) {
        this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      }
    }

    this.pointsOfInterestFiltered.sort((a, b) => {
      return this.compareStrings(a.name, b.name);
    });
  }

  /**
   * Sorts the filtered POI list by category. If already sorted by category, toggles the sort order.
   * @param keepSortOrder If true, keeps the current sort order instead of toggling it.
   */
  sortByCategory(keepSortOrder: boolean = false) {
    this.nameSort = false;
    this.detailsSort = false;

    if (!this.categorySort) {
      this.categorySort = true;
      this.sortOrder = 'asc';
    } else {
      if (!keepSortOrder) {
        this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      }
    }

    this.pointsOfInterestFiltered.sort((a, b) => {
      return this.compareStrings(a.category, b.category);
    });
  }

  /**
   * Sorts the filtered POI list by details. If already sorted by details, toggles the sort order.
   * @param keepSortOrder If true, keeps the current sort order instead of toggling it.
   */
  sortByDetails(keepSortOrder: boolean = false) {
    this.nameSort = false;
    this.categorySort = false;

    if (!this.detailsSort) {
      this.detailsSort = true;
      this.sortOrder = 'asc';
    } else {
      if (!keepSortOrder) {
        this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      }
    }

    this.pointsOfInterestFiltered.sort((a, b) => {
      return this.compareStrings(a.details, b.details);
    });
  }

  private compareStrings(a: string | undefined, b: string | undefined): number {
    const strA = a ? a.toLowerCase() : '';
    const strB = b ? b.toLowerCase() : '';

    if (this.sortOrder === 'asc') {
      return strA.localeCompare(strB);
    } else {
      return strB.localeCompare(strA);
    }
  }

  private updateFiltering() {
    this.poiFilterService.setFilterCriteria({ detailsFilter: this.detailsFilter, categoryFilter: this.categoryFilter, nameFilter: this.nameFilter });
    this.pointsOfInterestFiltered = this.poiFilterService.filter(this.pointsOfInterest, this.categoryFilter, this.nameFilter, this.detailsFilter);

    if (this.nameSort) {
      this.sortByName(true);
    }
    if (this.categorySort) {
      this.sortByCategory(true);
    }
    if (this.detailsSort) {
      this.sortByDetails(true);
    }
  }


  /**
   * Called after confirmation to really delete the selected POI.
   */
  confirmDeleteSelectedPoi(): void {
    if (this.selectedPoi) {
      this.deletePoi(this.selectedPoi);
      this.selectedPoi = undefined;
    }
    try {
      this.currentBsModal?.hide();
      this.currentBsModal = undefined;
    } catch (e) {
      // ignore
    }
  }

  /**
   * Opens the delete confirmation modal for the given POI.
   * @param poi 
   */
  async openDeleteConfirmationModal(poi: PointOfInterest) {
    this.selectedPoi = poi;
    // ensure Angular updates the modal bindings
    try { this.cd.detectChanges(); } catch (e) { /* ignore */ }

    // fallback to native confirm dialog if Bootstrap Modal is not available
    const fallback = () => {
      if (confirm(`Delete ${poi.name}?`)) this.deletePoi(poi);
    };

    const modalEl = document.getElementById('deleteConfirmationModal');
    const mod = await import('bootstrap/js/dist/modal').catch(() => undefined);
    const ModalClass = mod?.default ?? (window as any).bootstrap?.Modal;

    // Proceed only if we have both the modal element and the Bootstrap Modal class
    if (modalEl && ModalClass) {
      try {
        this.currentBsModal = new ModalClass(modalEl);
        this.currentBsModal.show();
      } catch (e) {
        // fallback if creation/showing fails
        fallback();
      }
    } else {
      // fallback to native confirm when modal or class isn't available
      fallback();
    }
  }

  /**
   * Calls the POI deletion request on the backend.
   * @param point
   */
  private deletePoi(point: PointOfInterest): void {
    // determine the time duration of the request
    const startTime = performance.now();

    this.poiService.deletePointOfInterest(point).subscribe({
      next: () => {
        const durationOfRequest = performance.now() - startTime;

        // Remove the deleted point from the local array
        this.pointsOfInterest = this.pointsOfInterest.filter(p => p !== point);
        this.pointsOfInterestFiltered = this.pointsOfInterest;

        this.showToastMessage(ToastNotification.titleDefault, //
          'Successfully deleted point of interest.',//
          durationOfRequest.toFixed(2) + ' ms', ToastNotification.cssClassSuccess);
      },
      error: (err) => {
        console.error('Error deleting point of interest:', err);
        this.showToastMessage(ToastNotification.titleDefault, 'Failed to delete the point of interest. Please try again later.', '', ToastNotification.cssClassError);
      }
    });
  }

}
