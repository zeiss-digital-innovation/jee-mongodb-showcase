import { PoiFilterService } from './poi-filter.service';
import { PointOfInterest } from '../model/point_of_interest';

describe('PoiFilterService', () => {
    let service: PoiFilterService;
    let sample: PointOfInterest[];

    beforeEach(() => {
        service = new PoiFilterService();
        sample = [
            { href: '', category: 'coffee', details: 'Nice place\nhttp://example.com', location: { coordinates: [13.7, 51.0], type: 'Point' } },
            { href: '', category: 'post', details: 'Post office', location: { coordinates: [13.8, 51.1], type: 'Point' } },
            { href: '', category: 'coffee', details: 'Best COFFEE in town', location: { coordinates: [13.9, 51.2], type: 'Point' } },
            { href: '', category: 'lodging', details: '', location: { coordinates: [14.0, 51.3], type: 'Point' } }
        ];
    });

    it('returns all points when no filters are provided', () => {
        const res = service.filter(sample, undefined, undefined);
        expect(res.length).toBe(sample.length);
    });

    it('filters by category (case-insensitive)', () => {
        const res = service.filter(sample, 'coFfEe', undefined);
        expect(res.length).toBe(2);
        expect(res.every(p => p.category?.toLowerCase() === 'coffee')).toBeTrue();
    });

    it('filters by details substring (case-insensitive)', () => {
        const res = service.filter(sample, undefined, 'http');
        expect(res.length).toBe(1);
        expect(res[0].details).toContain('http://example.com');
    });

    it('applies both category and details filters', () => {
        const res = service.filter(sample, 'coffee', 'best');
        expect(res.length).toBe(1);
        expect(res[0].category).toBe('coffee');
        expect(res[0].details?.toLowerCase()).toContain('best');
    });

    it('empty string filters are treated as not provided', () => {
        // empty strings are falsy in the filter implementation
        const res1 = service.filter(sample, '', '');
        expect(res1.length).toBe(sample.length);
    });

    it('returns empty array when input is empty', () => {
        const res = service.filter([], 'coffee', 'http');
        expect(res.length).toBe(0);
    });
});
