import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PointOfInterestService } from '../service/point-of-interest.service';
import { PointOfInterest } from '../model/point_of_interest';
import { FormatDetailsPipe } from '../pipe/format-details-pipe';

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
  radiusDefault = 2000; // in meters

  latitude: number = this.latitudeDefault;
  longitude: number = this.longitudeDefault;
  radius: number = this.radiusDefault;

  pointsOfInterest: PointOfInterest[] = [];

  constructor(private poiService: PointOfInterestService) { }

  ngOnInit(): void {
    // Example coordinates and radius
    const latitude = 51.0504; // Replace with actual latitude
    const longitude = 13.7373; // Replace with actual longitude
    const radius = 1000; // Replace with actual radius in meters

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
    // Implement edit functionality here
    console.log('Edit POI:', point);
  }

  deletePoi(point: PointOfInterest): void {
    // Implement delete functionality here
    console.log('Delete POI:', point);
  }
}
