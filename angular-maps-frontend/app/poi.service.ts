import {Http} from '@angular/http';
import {Injectable} from '@angular/core';
import 'rxjs/add/operator/map'; // add map function to observable

@Injectable()
export class PoiService {

    constructor(private http: Http) {
    }

    getPoiList(lat: number, lng: number) {
      //console.log('Service: ' + lat + ' ' + lng);

      return this.http.get('http://localhost:8080/geoservice/rest/poi?lat=' + lat + '&lon=' + lng + '&radius=3000')
           .map(res => res.json());

      /*
      if (lat < 51.03507935246506) {
        return this.http.get('assets/pois.json')
             .map(res => res.json());
      } else {
        return this.http.get('assets/pois_2.json')
             .map(res => res.json());
      }
      */

    }
}
