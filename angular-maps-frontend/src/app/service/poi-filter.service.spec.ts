import { PoiFilterService } from './poi-filter.service';
import { PointOfInterest } from '../model/point_of_interest';

describe('PoiFilterService', () => {
    let service: PoiFilterService;
    let sample: PointOfInterest[];

    beforeEach(() => {
        service = new PoiFilterService();
        sample = [
            { href: '', category: 'coffee', name: 'Coffeeshop', details: 'Nice place\nhttp://example.com', location: { coordinates: [13.7, 51.0], type: 'Point' } },
            { href: '', category: 'post', name: 'Post Office', details: 'Post office', location: { coordinates: [13.8, 51.1], type: 'Point' } },
            { href: '', category: 'coffee', name: 'Coffeeshop 2', details: 'Best COFFEE in town', location: { coordinates: [13.9, 51.2], type: 'Point' } },
            { href: '', category: 'lodging', name: 'Best Hotel', details: '', location: { coordinates: [14.0, 51.3], type: 'Point' } }
        ];
    });

    it('returns all points when no filters are provided', () => {
        const res = service.filter(sample, undefined, undefined, undefined);
        expect(res.length).toBe(sample.length);
    });

    it('filters by category (case-insensitive)', () => {
        const res = service.filter(sample, 'coFfEe', undefined, undefined);
        expect(res.length).toBe(2);
        expect(res.every(p => p.category?.toLowerCase() === 'coffee')).toBeTrue();
    });

    it('filters by name substring (case-insensitive)', () => {
        const res = service.filter(sample, undefined, 'shop', undefined);
        expect(res.length).toBe(2);
        expect(res[0].name).toContain('Coffeeshop');
        expect(res[1].name).toContain('Coffeeshop');
    });

    it('filters by details substring (case-insensitive)', () => {
        const res = service.filter(sample, undefined, undefined, 'http');
        expect(res.length).toBe(1);
        expect(res[0].details).toContain('http://example.com');
    });

    it('applies category and name filters', () => {
        const res = service.filter(sample, 'coffee', '2', undefined);
        expect(res.length).toBe(1);
        expect(res[0].category).toBe('coffee');
        expect(res[0].name).toContain('2');
    });

    it('applies category and details filters', () => {
        const res = service.filter(sample, 'coffee', undefined, 'best');
        expect(res.length).toBe(1);
        expect(res[0].category).toBe('coffee');
        expect(res[0].details?.toLowerCase()).toContain('best');
    });

    // apply filter for category, name and details
    it('applies category, name and details filters', () => {
        const res = service.filter(sample, 'coffee', '2', 'best');
        expect(res.length).toBe(1);
        expect(res[0].category).toBe('coffee');
        expect(res[0].name).toContain('2');
        expect(res[0].details?.toLowerCase()).toContain('best');
    });

    it('empty string filters are treated as not provided', () => {
        // empty strings are falsy in the filter implementation
        const res1 = service.filter(sample, '', '', '');
        expect(res1.length).toBe(sample.length);
    });

    it('returns empty array when input is empty', () => {
        const res = service.filter([], 'coffee', 'name', 'http');
        expect(res.length).toBe(0);
    });

    it('noCategoryFilterSet should return true for undefined/empty/Choose...', () => {
        expect(service.noCategoryFilterSet(undefined)).toBeTrue();

        expect(service.noCategoryFilterSet('')).toBeTrue();

        expect(service.noCategoryFilterSet('Choose...')).toBeTrue();

        expect(service.noCategoryFilterSet('coffee')).toBeFalse();
    });

    it('matchesCategoryCriteria should compare case-insensitively and handle undefined', () => {
        expect(service.matchesCategoryCriteria('coffee', undefined)).toBeFalse();

        expect(service.matchesCategoryCriteria('coffee', 'CoFfEe')).toBeTrue();
        expect(service.matchesCategoryCriteria('CoFfEe', 'CoFfEe')).toBeTrue();

        expect(service.matchesCategoryCriteria('post', 'coffee')).toBeFalse();
        expect(service.matchesCategoryCriteria(undefined, 'coffee')).toBeFalse();
    });
});
