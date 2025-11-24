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

    /**
     * Mapping of zoom levels to search radius in meters.
     * Used by mapZoomToRadius() and mapRadiusToZoom() methods.
     */
    private readonly zoomToRadiusMap = new Map<number, number>([
        [9, 50000],
        [10, 30000],
        [11, 20000],
        [12, 10000],
        [13, 3000],
        [14, 2000],
        [15, 1000],
    ]);

    constructor(private sanitizer: Sanitizer, private poiFormat: PoiFormatService) { }

    /**
     * Creates the popup content for a given point of interest.
     * @param poi Point of interest
     * @returns Popup content.
     */
    getMarkerPopupFor(poi: PointOfInterest): string {
        var markerPopup = '';

        if (!poi) return markerPopup;

        const cat = (poi.category || '').toLowerCase();

        markerPopup += '<div class="poi-description-header">';
        // Name: Left-aligned using flex default, displays with .lead class (larger size)
        markerPopup += `<span class="lead">${this.poiFormat.formatDetails(poi.name || 'Unnamed')}</span>`;
        markerPopup += `<i class="${getBootstrapIconClass(cat)} text-primary poi-description-header-icon"></i>`;
        markerPopup += '</div>';

        // Coordinates line with geo icon and formatted lat/lon (4 decimal places)
        const lat = poi.location.coordinates[1].toFixed(4);
        const lng = poi.location.coordinates[0].toFixed(4);
        markerPopup += `<div class="poi-description-coordinates"><i class="bi bi-geo-alt text-primary"></i>&nbsp;Lat: ${lat} | Lng: ${lng}</div>`;

        // sanitize and format details: only allow safe links, escape other text, format phones
        let details = this.poiFormat.formatDetails(poi.details || '');

        markerPopup += `<div class="poi-description-details">${details}</div>`;

        // Category badge
        markerPopup += '<div class="poi-description-category"><span class="badge bg-secondary">' + cat.charAt(0).toUpperCase() + cat.slice(1) + '</span></div>';

        return markerPopup;
    }

    /**
     * Maps a zoom level to the corresponding search radius using the zoom-to-radius map.
     * @param zoom Zoom level.
     * @returns Search radius in meters, or a default radius if zoom is not in the map.
     */
    mapZoomToRadius(zoom: number): number {
        // Check if the zoom level exists in the map
        if (this.zoomToRadiusMap.has(zoom)) {
            return this.zoomToRadiusMap.get(zoom)!;
        }

        // For zoom levels between mapped values, find the closest lower zoom and return its radius
        const sortedZooms = Array.from(this.zoomToRadiusMap.keys()).sort((a, b) => a - b);
        const closestZoom = sortedZooms.reverse().find(z => z < zoom);

        if (closestZoom !== undefined) {
            return this.zoomToRadiusMap.get(closestZoom)!;
        }

        // If zoom is less than the smallest mapped zoom, return the radius for the smallest zoom
        const minZoom = Math.min(...Array.from(this.zoomToRadiusMap.keys()));
        return this.zoomToRadiusMap.get(minZoom)!;
    }

    /**
     * Maps a radius to the corresponding zoom level by finding the closest radius in the map.
     * @param radius Search radius in meters.
     * @returns Zoom level that best matches the given radius.
     */
    mapRadiusToZoom(radius: number): number {
        let closestZoom = 13; // default zoom
        let minDifference = Number.MAX_VALUE;

        // Iterate through the map and find the radius value closest to the input
        for (const [zoom, mapRadius] of this.zoomToRadiusMap.entries()) {
            const difference = Math.abs(mapRadius - radius);

            if (difference < minDifference) {
                minDifference = difference;
                closestZoom = zoom;
            }
        }

        return closestZoom;
    }

}