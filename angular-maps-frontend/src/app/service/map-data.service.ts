import { Injectable } from "@angular/core";
import { PointOfInterest } from "../model/point_of_interest";
import { Sanitizer } from '../util/sanitization.util';
import { PoiFormatService } from './poi-format.service';
import { getBootstrapIconClass } from "../model/poi-categories";

/**
 * Service for map related data (i.e. popup content) and calculations.
 */
@Injectable({
    providedIn: 'root',
})
export class MapDataService {

    constructor(private sanitizer: Sanitizer, private poiFormat: PoiFormatService) { }

    /**
     * Creates the popup content for a given point of interest.
     * @param poi Point of interest
     * @returns Popup content.
     */
    getMarkerPopupFor(poi: PointOfInterest): string {
        var markerPopup = '';

        if (!poi) return markerPopup;

        var iconImg: string;

        const cat = (poi.category || '').toLowerCase();

        iconImg = `<i class="${getBootstrapIconClass(cat)} text-primary"></i>`;

        markerPopup += iconImg + '&nbsp;<strong>' + cat.charAt(0).toUpperCase() + cat.slice(1) + '</strong><br />';

        markerPopup += `<br><span class="lead">${this.poiFormat.formatDetails(poi.name || 'Unnamed')}</span><br />`;

        // sanitize and format details: only allow safe links, escape other text, format phones
        let details = this.poiFormat.formatDetails(poi.details || '');

        markerPopup += `<br>${details}`;

        return markerPopup;
    }

    /**
     * Determines the search radius for points of interest based on the current zoom level.
     * @param zoom Current map zoom level.
     * @returns Search radius in meters.
     */
    getRadiusForZoom(zoom: number): number {
        if (zoom <= 8) {
            return 50000;
        }
        if (zoom <= 11) {
            return 20000;
        }
        if (zoom == 12) {
            return 10000;
        }
        if (zoom == 13) {
            return 3000;
        }
        return 2000;
    }

}