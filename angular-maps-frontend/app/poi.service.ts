import {Http} from '@angular/http';
import {Injectable} from '@angular/core';
import 'rxjs/add/operator/map'; // add map function to observable

@Injectable()
export class PoiService {

    constructor(private http: Http) {
    }

    getPoi(href: string) {
      return this.http.get(href).map(res => res.json());
    }

    getPoiList(lat: number, lng: number) {
      return this.http.get('http://localhost:8080/geoservice/rest/poi?lat=' + lat + '&lon=' + lng + '&radius=5000')
           .map(res => res.json());
    }
}
