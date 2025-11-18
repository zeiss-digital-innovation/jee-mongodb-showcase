import { MapDataService } from './map-data.service';
import { PointOfInterest } from '../model/point_of_interest';
import { Sanitizer } from '../util/sanitization.util';
import { PoiFormatService } from './poi-format.service';

describe('MapDataService', () => {
    let service: MapDataService;
    let sanitizer: Sanitizer;
    let poiFormat: PoiFormatService;

    beforeEach(() => {
        sanitizer = new Sanitizer();
        poiFormat = new PoiFormatService(sanitizer);
        service = new MapDataService(sanitizer, poiFormat);
    });

    describe('getMarkerPopupFor', () => {
        it('should escape malicious details and not include script tags', () => {
            const poi: PointOfInterest = {
                href: '',
                category: 'coffee',
                name: 'name',
                details: 'Nice place\n<script>alert(1)</script>',
                location: { coordinates: [13.7, 51.0], type: 'Point' }
            };

            const res = service.getMarkerPopupFor(poi);
            expect(res).not.toContain('<script>');
            expect(res).toContain('Nice place');
            expect(res).toContain('&lt;script&gt;');
        });

        it('should render safe links as anchors', () => {
            const poi: PointOfInterest = {
                href: '',
                category: 'post',
                name: 'name',
                details: 'http://example.com',
                location: { coordinates: [13.7, 51.0], type: 'Point' }
            };

            const res = service.getMarkerPopupFor(poi);
            expect(res).toContain('<a');
            expect(res).toContain('http://example.com');
        });
    });

    describe('mapZoomToRadius', () => {
        it('should return the correct radius for a mapped zoom level', () => {
            // Test basic mapping for exact zoom levels in the map
            expect(service.mapZoomToRadius(9)).toBe(50000);
            expect(service.mapZoomToRadius(11)).toBe(20000);
            expect(service.mapZoomToRadius(13)).toBe(3000);
            expect(service.mapZoomToRadius(15)).toBe(1000);
        });

        it('should return the closest lower zoom radius for unmapped zoom levels', () => {
            // For zoom 16 (not in map), should return radius for zoom 15
            expect(service.mapZoomToRadius(16)).toBe(1000);

            // For zoom 14.5 (not in map), should return radius for zoom 14
            expect(service.mapZoomToRadius(14.5)).toBe(2000);
        });

        it('should return the largest radius for zoom levels below the minimum mapped zoom', () => {
            // For zoom levels below the minimum mapped zoom (9),
            // the method should return the radius for the smallest zoom level (9 -> 50000)
            expect(service.mapZoomToRadius(8)).toBe(50000);
            expect(service.mapZoomToRadius(0)).toBe(50000);
        });
    });

    describe('mapRadiusToZoom', () => {
        it('should return the correct zoom for exact mapped radius values', () => {
            // Test basic mapping for exact radius values in the map
            expect(service.mapRadiusToZoom(50000)).toBe(9);
            expect(service.mapRadiusToZoom(20000)).toBe(11);
            expect(service.mapRadiusToZoom(3000)).toBe(13);
            expect(service.mapRadiusToZoom(1000)).toBe(15);
        });

        it('should find the closest zoom for radius values between two mapped values', () => {
            // Test radius values that fall between mapped values
            // Radius 5000 is between 3000 (zoom 13) and 10000 (zoom 12)
            // Distance to 3000: |5000 - 3000| = 2000
            // Distance to 10000: |5000 - 10000| = 5000
            // Should return zoom 13 (closer)
            expect(service.mapRadiusToZoom(5000)).toBe(13);

            // Radius 15000 is between 10000 (zoom 12) and 20000 (zoom 11)
            // Distance to 10000: |15000 - 10000| = 5000
            // Distance to 20000: |15000 - 20000| = 5000
            // When distances are equal, the first encountered wins (implementation dependent)
            // This should be either 12 or 11; based on map iteration order
            const result = service.mapRadiusToZoom(15000);
            expect([11, 12]).toContain(result);

            // Radius 40000 is between 30000 (zoom 10) and 50000 (zoom 9)
            // Distance to 30000: |40000 - 30000| = 10000
            // Distance to 50000: |40000 - 50000| = 10000
            // Should return either 9 or 10
            const result2 = service.mapRadiusToZoom(40000);
            expect([9, 10]).toContain(result2);
        });

        it('should find the closest zoom for arbitrary radius values', () => {
            // Test with non-map radius values
            // Radius 1200 is between 1000 (zoom 15) and 2000 (zoom 14)
            // Distance to 1000: |1200 - 1000| = 200
            // Distance to 2000: |1200 - 2000| = 800
            // Should map to zoom 15 (1000 is closer)
            expect(service.mapRadiusToZoom(1200)).toBe(15);

            // Radius 4000 is between 3000 (zoom 13) and 10000 (zoom 12)
            // Distance to 3000: |4000 - 3000| = 1000
            // Distance to 10000: |4000 - 10000| = 6000
            // Should map to zoom 13 (3000 is closer)
            expect(service.mapRadiusToZoom(4000)).toBe(13);

            // Radius 25000 is between 20000 (zoom 11) and 30000 (zoom 10)
            // Distance to 20000: |25000 - 20000| = 5000
            // Distance to 30000: |25000 - 30000| = 5000
            // Distances are equal, so result depends on map iteration order
            const result = service.mapRadiusToZoom(25000);
            expect([10, 11]).toContain(result);

            // Radius 100000 is closest to 50000 (zoom 9)
            expect(service.mapRadiusToZoom(100000)).toBe(9);
        });

        it('should handle very small and very large radius values', () => {
            // Very small radius should map to the smallest available zoom (highest detail)
            expect(service.mapRadiusToZoom(100)).toBe(15);

            // Very large radius should map to the largest available zoom (lowest detail)
            expect(service.mapRadiusToZoom(500000)).toBe(9);
        });
    });

    describe('mapZoomToRadius and mapRadiusToZoom round-trip', () => {
        it('should map zoom to radius and back for mapped values', () => {
            // Test round-trip for exact mapped zoom values
            const zoom13 = 13;
            const radius = service.mapZoomToRadius(zoom13);
            const zoomBack = service.mapRadiusToZoom(radius);

            expect(radius).toBe(3000);
            expect(zoomBack).toBe(zoom13);
        });

        it('should maintain approximate zoom levels on round-trip for unmapped zooms', () => {
            // For unmapped zoom values, the round-trip may not be exact
            // but should map back to a reasonable zoom level
            const zoom16 = 16;
            const radius = service.mapZoomToRadius(zoom16); // Should be 1000
            const zoomBack = service.mapRadiusToZoom(radius);

            expect(radius).toBe(1000);
            expect(zoomBack).toBe(15); // Maps back to the closest zoom
        });
    });
});
