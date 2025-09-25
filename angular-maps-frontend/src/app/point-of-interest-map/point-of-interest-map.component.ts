import { Component, OnInit } from '@angular/core';
import { PointOfInterest } from '../model/point_of_interest';
import { PointOfInterestService } from '../service/point-of-interest.service';
import * as L from 'leaflet';
import { MapDataService } from '../service/map-data.service';
import { CommonModule } from '@angular/common';

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
  categories: string[] = ['Restaurant', 'Cash', 'Supermarket', 'Post', 'Lodging', 'Police', 'Toilet', 'Coffee', 'Parking', 'Gas Station', 'Company', 'Pharmacy'];
  map: L.Map | undefined;

  latitudeDefault = 51.0504;
  longitudeDefault = 13.7373;
  zoomDefault = 13;

  constructor(private poiService: PointOfInterestService, private mapDataService: MapDataService) {
    //
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
