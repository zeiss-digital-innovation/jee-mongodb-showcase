import { MapDataService } from './map-data.service';
import { PointOfInterest } from '../model/point_of_interest';

describe('MapDataService', () => {
    let service: MapDataService;

    beforeEach(() => {
        service = new MapDataService();
    });

    describe('formatForLink', () => {
        it('should return an anchor for http url', () => {
            const res = service.formatForLink('http://example.com');
            expect(res).toContain('<a');
            expect(res).toContain('http://example.com');
        });

        it('should return an anchor for www url and add https prefix', () => {
            const res = service.formatForLink('www.example.com');
            expect(res).toContain('<a');
            expect(res).toContain('https://www.example.com');
        });

        it('should escape html for non-url text', () => {
            const res = service.formatForLink('<script>alert(1)</script>');
            expect(res).not.toContain('<script>');
            expect(res).toContain('&lt;script&gt;');
        });
    });

    describe('formatForPhone', () => {
        it('should prefix +49 numbers with phone icon and escape', () => {
            const res = service.formatForPhone('+49123456789');
            expect(res).toContain('+49123456789');
            expect(res).toContain('svg');
        });

        it('should handle Tel.: prefix', () => {
            const res = service.formatForPhone('Tel.: 0123');
            expect(res).toContain('0123');
            expect(res).toContain('svg');
        });

        it('should escape arbitrary text', () => {
            const res = service.formatForPhone('<b>bold</b>');
            expect(res).not.toContain('<b>');
            expect(res).toContain('&lt;b&gt;bold&lt;/b&gt;');
        });
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
