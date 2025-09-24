import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PointOfInterestService } from '../service/point-of-interest.service';
import { PointOfInterest } from '../model/point_of_interest';

@Component({
  selector: 'app-point-of-interest-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './point-of-interest-list.component.html',
  styleUrl: './point-of-interest-list.component.css'
})
export class PointOfInterestListComponent implements OnInit {

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
}
