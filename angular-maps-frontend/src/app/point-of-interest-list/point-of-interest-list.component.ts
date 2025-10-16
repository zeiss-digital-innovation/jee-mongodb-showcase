import { Component, OnInit } from '@angular/core';
import { ApplicationRef, createComponent, EnvironmentInjector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PointOfInterestService } from '../service/point-of-interest.service';
import { PointOfInterest } from '../model/point_of_interest';
import { POI_CATEGORIES } from '../model/poi-categories';
import { FormatDetailsPipe } from '../pipe/format-details-pipe';
import { PoiDialogComponent } from '../poi-dialog/poi-dialog.component';

@Component({
  selector: 'app-point-of-interest-list',
  standalone: true,
  imports: [CommonModule, FormsModule, FormatDetailsPipe],
  templateUrl: './point-of-interest-list.component.html',
  styleUrl: './point-of-interest-list.component.css'
})
export class PointOfInterestListComponent implements OnInit {

  latitudeDefault = 51.0504;
  longitudeDefault = 13.7373;
  radiusDefault = 1000; // in meters

  latitude: number = this.latitudeDefault;
  longitude: number = this.longitudeDefault;
  radius: number = this.radiusDefault;

  pointsOfInterest: PointOfInterest[] = [];

  constructor(private poiService: PointOfInterestService, private appRef: ApplicationRef, private injector: EnvironmentInjector) { }

  ngOnInit(): void {
    // Example coordinates and radius
    const latitude = this.latitudeDefault; // Replace with actual latitude
    const longitude = this.longitudeDefault; // Replace with actual longitude
    const radius = this.radiusDefault; // Replace with actual radius in meters

    this.poiService.getPointsOfInterest(latitude, longitude, radius)
      .subscribe(points => {
        this.pointsOfInterest = points;
      });
  }

  setRadius(event: Event): void {
    this.radius = Number((event.target as HTMLInputElement).value);
  }

  loadPointsOfInterest(): void {
    //console.log(`Loading POIs for lat=${this.latitude}, lon=${this.longitude}, radius=${this.radius}`);

    this.poiService.getPointsOfInterest(this.latitude, this.longitude, this.radius)
      .subscribe(points => {
        this.pointsOfInterest = points;
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
      cleanupComponent();
    });
  }

  deletePoi(point: PointOfInterest): void {
    if (confirm('Are you sure you want to delete this point of interest?\n' + point.details)) {
      this.poiService.deletePointOfInterest(point).subscribe({
        next: () => {
          // Remove the deleted point from the local array
          this.pointsOfInterest = this.pointsOfInterest.filter(p => p !== point);
        },
        error: (err) => {
          console.error('Error deleting point of interest:', err);
          alert('Failed to delete the point of interest. Please try again.');
        }
      });
    }
  }
}
