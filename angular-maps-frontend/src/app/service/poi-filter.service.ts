import { Injectable } from "@angular/core";
import { PointOfInterest } from "../model/point_of_interest";

@Injectable({ providedIn: 'root' })
export class PoiFilterService {

    filter(pointsOfInterest: PointOfInterest[], categoryFilter: string | undefined, detailsFilter: string | undefined): PointOfInterest[] {
        return pointsOfInterest.filter(poi => {
            const matchesCategory = categoryFilter ? poi.category?.toLowerCase() === categoryFilter.toLowerCase() : true;
            const matchesDetails = detailsFilter ? poi.details?.toLowerCase().includes(detailsFilter.toLowerCase()) : true;
            return matchesCategory && matchesDetails;
        });
    }
}