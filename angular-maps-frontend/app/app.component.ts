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
      height: 600px;
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

  public poiList = [];

  constructor(private poiService: PoiService) {

    this.loadData();
  }

  processCenterChange(coordinates : LatLngLiteral) {
	  this.latQuery = coordinates.lat;
	  this.lngQuery = coordinates.lng;
  }

  doQuery() {
	  this.loadData();
  }

  loadData() {
	   var featureCollection;
     this.poiService.getPoiList(this.latQuery, this.lngQuery).subscribe(resultList => this.poiList = resultList);
  }
}
