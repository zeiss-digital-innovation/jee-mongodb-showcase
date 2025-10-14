import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { PointOfInterest } from '../model/point_of_interest';
import { Sanitizer } from '../util/sanitization.util';

@Injectable({
    providedIn: "root"
})
export class PointOfInterestService {

    constructor(private http: HttpClient, private sanitizer: Sanitizer) { }

    /**
     * Retrieves all points of interest from the backend REST service
     * @returns Observable<PointOfInterest[]> - Array of points of interest
     */
    getPointsOfInterest(latitude: number, longitude: number, radius: number): Observable<PointOfInterest[]> {
        if (!latitude || !longitude || !radius) {
            throw new Error('Invalid parameters');
        }

        //return getMockPointsOfInterest();
        // Note: The BaseUrlInterceptor will prepend the base URL
        return this.http.get<PointOfInterest[]>(`/poi?lat=${latitude}&lon=${longitude}&radius=${radius}&expand=details`);
    }

    /**
     * Create a new point of interest on the backend
     * The BaseUrlInterceptor will prepend the configured base URL.
     */
    createPointOfInterest(point: PointOfInterest): Observable<PointOfInterest> {
        const sanitized = this.sanitizer.sanitizePoint(point);
        return this.http.post<PointOfInterest>(`/poi`, sanitized);
    }



}

// Use this function to work with mock data (i.e. without a running backend).
function getMockPointsOfInterest(): Observable<PointOfInterest[]> {
    const mockData: PointOfInterest[] = [
        {
            "category": "police",
            "details": "Polizei\nD-Dresden\nSchießgasse 7",
            "href": "http://localhost:8080/geoservice/rest/poi/576840091921af14e485a872",
            "location": {
                "coordinates": [
                    13.74412,
                    51.05058
                ],
                "type": "Point"
            }
        },
        {
            "category": "cash",
            "details": "Sparkasse ATM\nD-Dresden\nWebergasse 1 Altmarkt-Galerie",
            "href": "http://localhost:8080/geoservice/rest/poi/57683c861921af14e4840e77",
            "location": {
                "coordinates": [
                    13.73699,
                    51.04925
                ],
                "type": "Point"
            }
        },
        {
            "category": "restaurant",
            "details": "McDonald's\nD-Dresden\nWilsdruffer Str. 19",
            "href": "http://localhost:8080/geoservice/rest/poi/5768403a1921af14e485bdc8",
            "location": {
                "coordinates": [
                    13.73914,
                    51.04981
                ],
                "type": "Point"
            }
        },
        {
            "category": "toilet",
            "details": "Starbucks\nD-Dresden\nAltmarkt\n+49 351 43833967",
            "href": "http://localhost:8080/geoservice/rest/poi/576840431921af14e485c1ee",
            "location": {
                "coordinates": [
                    13.73863,
                    51.04929
                ],
                "type": "Point"
            }
        },
        {
            "category": "coffee",
            "details": "McDonald's\nD-Dresden\nAltmarkt-Galerie Im Untergeschoß",
            "href": "http://localhost:8080/geoservice/rest/poi/576840391921af14e485bd98",
            "location": {
                "coordinates": [
                    13.73526,
                    51.04918
                ],
                "type": "Point"
            }
        },
        {
            "category": "parking",
            "details": "Cash Group (Postbank)\nD-Dresden\nWebergasse 1",
            "href": "http://localhost:8080/geoservice/rest/poi/57683bef1921af14e483ca40",
            "location": {
                "coordinates": [
                    13.73505,
                    51.0492
                ],
                "type": "Point"
            }
        },
        {
            "category": "supermarket",
            "details": "Aldi\nD-Dresden-Innere Altstadt\nWebergasse 1",
            "href": "http://localhost:8080/geoservice/rest/poi/5768405a1921af14e485cc2e",
            "location": {
                "coordinates": [
                    13.73487,
                    51.04927
                ],
                "type": "Point"
            }
        },
        {
            "category": "post",
            "details": "Deutsche Post\nD-Dresden\nWebergasse 1",
            "href": "http://localhost:8080/geoservice/rest/poi/576840201921af14e485b252",
            "location": {
                "coordinates": [
                    13.73437,
                    51.04934
                ],
                "type": "Point"
            }
        },
        {
            "category": "lodging",
            "details": "Kempinski Hotel Taschenbergpalais\nD-Deutschland\n+49351-491200",
            "href": "http://localhost:8080/geoservice/rest/poi/57683e631921af14e484e81e",
            "location": {
                "coordinates": [
                    13.73546,
                    51.05226
                ],
                "type": "Point"
            }
        }, {
            "category": "gas_station",
            "details": "The Westin Bellevue Dresden\nD-Deutschland\n+49351-8050",
            "href": "http://localhost:8080/geoservice/rest/poi/57683e921921af14e484fdbe",
            "location": {
                "coordinates": [
                    13.739,
                    51.05877
                ],
                "type": "Point"
            }
        },
        {
            "category": "company",
            "details": "Carl Zeiss Digital Innovation GmbH\nFritz-Foerster-Platz 2\n01069 Dresden\nTel.: +49 (0)351 497 01-500\nFax: +49 (0)351 497 01-589",
            "href": "http://localhost:8080/geoservice/rest/poi/57683cd41921af14e4843289",
            "location": {
                "coordinates": [
                    13.730123,
                    51.030782
                ],
                "type": "Point"
            }
        }
    ];
    return new Observable<PointOfInterest[]>(observer => {
        observer.next(mockData);
        observer.complete();
    });
}

