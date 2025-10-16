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
                details: 'http://example.com',
                location: { coordinates: [13.7, 51.0], type: 'Point' }
            };

            const res = service.getMarkerPopupFor(poi);
            expect(res).toContain('<a');
            expect(res).toContain('http://example.com');
        });
    });
});
