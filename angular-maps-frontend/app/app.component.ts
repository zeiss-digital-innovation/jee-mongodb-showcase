import {Component} from '@angular/core';
import {SebmGoogleMap, SebmGoogleMapMarker, SebmGoogleMapInfoWindow, LatLngLiteral} from 'angular2-google-maps/core';
import {HTTP_PROVIDERS} from '@angular/http';

import {PoiService} from './poi.service';

@Component({
  selector: 'my-app',
  directives: [SebmGoogleMap, SebmGoogleMapMarker, SebmGoogleMapInfoWindow], // this loads all angular2-google-maps directives in this component
  providers: [PoiService, HTTP_PROVIDERS],
  // the following line sets the height of the map - Important: if you don't set a height, you won't see a map!!
  styles: [`
    .sebm-google-map-container {
      height: 500px;
    }
  `],
  templateUrl: 'templates/map.html'
})
export class AppComponent{
  lat: number = 51.030812;
  lng: number = 13.730180;
  zoom: number = 15;
  latQuery: number = 51.030812;
  lngQuery: number = 13.730180;
  latQueryLast: number;
  lngQueryLast: number;
  markerInfoText : string;

  public poiList = [];
  //public selectedPoi : object;

  constructor(private poiService: PoiService) {

    this.loadData();
  }

  processCenterChange(coordinates : LatLngLiteral) {
	  this.latQuery = coordinates.lat;
	  this.lngQuery = coordinates.lng;
  }

  processMarkerClick(poi) {
    this.poiService.getPoi(poi.href).subscribe(p => poi.details = p.details.replace(/\n/g, "<br />"));
  }

  getIconUrl(category : string) {
    // find icons at: https://sites.google.com/site/gmapsdevelopment/

    if (category == "gasstation") {
      return "http://maps.google.com/mapfiles/ms/micons/gas.png";
    } else if (category == "supermarket") {
      return "http://maps.google.com/mapfiles/ms/micons/convienancestore.png";
    } else if (category == "restaurant") {
      return "http://maps.google.com/mapfiles/ms/micons/restaurant.png";
    } else if (category == "cash") {
      return "http://maps.google.com/mapfiles/ms/micons/dollar.png";
    } else if (category == "parking") {
      return "http://maps.google.com/mapfiles/ms/micons/parkinglot.png";
    } else if (category == "coffee") {
      return "http://maps.google.com/mapfiles/ms/micons/coffeehouse.png";
    } else if (category == "pharmacy") {
      return "images/pharmacy.png";
    } else if (category == "company") {
      return "images/saxsys_logo_map.png";
    }
    return "http://maps.google.com/mapfiles/ms/micons/red.png";
  }

  doQuery() {
    // only reload from server if things changed
    if (this.latQueryLast != this.latQuery && this.lngQueryLast != this.latQuery) {
      this.latQueryLast = this.latQuery;
      this.lngQueryLast = this.lngQuery;

      this.loadData();
    }
  }

  loadData() {
	   var featureCollection;
     this.poiService.getPoiList(this.latQuery, this.lngQuery).subscribe(resultList => this.poiList = resultList);
  }
}
