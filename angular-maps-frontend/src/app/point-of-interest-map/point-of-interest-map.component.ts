import { Component, OnInit } from '@angular/core';
import { PointOfInterest } from '../model/point_of_interest';
import { PointOfInterestService } from '../service/point-of-interest.service';
import * as L from 'leaflet';
import { MapDataService } from '../service/map-data.service';

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
  imports: [],
  templateUrl: './point-of-interest-map.component.html',
  styleUrl: './point-of-interest-map.component.css'
})
export class PointOfInterestMapComponent implements OnInit {

  pointsOfInterest: PointOfInterest[] = [];
  map: L.Map | undefined;

  constructor(private poiService: PointOfInterestService, private mapDataService: MapDataService) {
    //
  }

  ngOnInit(): void {
    // Example coordinates and radius
    const latitude = 51.0504; // Replace with actual latitude
    const longitude = 13.7373; // Replace with actual longitude
    const radius = 1000; // Replace with actual radius in meters

    // Initialize the map
    this.map = L.map('map').setView([latitude, longitude], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(this.map);

    this.loadPointsOfInterest(latitude, longitude, radius);
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
    this.pointsOfInterest.forEach(poi => {
      const coords = poi.location.coordinates;

      L.marker([coords[1], coords[0]]).addTo(this.map!)
        .bindPopup(this.mapDataService.getMarkerPopupFor(poi));
    });
  }

}
