import { Injectable } from "@angular/core";
import { PointOfInterest } from "../model/point_of_interest";
import { Sanitizer } from '../util/sanitization.util';
import { PoiFormatService } from './poi-format.service';

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
        if (!poi) return '';

        var iconImg: string;
        const category = (poi.category || '').toLowerCase();

        switch (category) {
            case 'cash':
                iconImg =
                    `<i class="bi-credit-card"></i>`;
                break;
            case 'coffee':
                iconImg =
                    `<i class="bi-cup-hot"></i>`;
                break;
            case 'company':
                iconImg =
                    `<i class="bi-building"></i>`;
                break;
            case 'gasstation':
                iconImg =
                    `<i class="bi-fuel-pump"></i>`;
                break;
            case 'lodging':
                iconImg =
                    `<i class="bi-house"></i>`;
                break;
            case 'parking':
                iconImg =
                    `<i class="bi-car-front"></i>`;
                break;
            case 'pharmacy':
                iconImg =
                    `<i class="bi-plus-square"></i>`;
                break;
            case 'police':
                iconImg =
                    `<i class="bi-shield-check"></i>`;
                break;
            case 'post':
                iconImg =
                    `<i class="bi-mailbox"></i>`;
                break;
            case 'restaurant':
                iconImg =
                    `<i class="bi-cup-hot"></i>`;
                break;
            case 'supermarket':
                iconImg =
                    `<i class="bi-shop"></i>`;
                break;
            case 'toilet':
                iconImg =
                    `<i class="bi-person-standing"></i>`;
                break;
            default:
                iconImg =
                    `<i class="bi-geo-alt"></i>`;
        }

        // sanitize and format details: only allow safe links, escape other text, format phones
        let details = this.poiFormat.formatDetails(poi.details || '');

        iconImg += `<br>${details}`;

        return iconImg;
    }





    /**
     * Determines the search radius for points of interest based on the current zoom level.
     * @param zoom Current map zoom level.
     * @returns Search radius in meters.
     */
    getRadiusForZoom(zoom: number): number {
        if (zoom <= 8) {
            return 20000;
        }
        if (zoom <= 11) {
            return 10000;
        }
        if (zoom == 12) {
            return 5000;
        }
        if (zoom == 13) {
            return 2000;
        }
        return 1000;
    }

}