import { Sanitizer } from './sanitization.util';
import { PointOfInterest } from '../model/point_of_interest';

describe('Sanitizer', () => {
    let s: Sanitizer;

    beforeEach(() => s = new Sanitizer());

    it('escapes html in name, details and href', () => {
        const poi: PointOfInterest = { href: '<script>', category: 'cat', name: 'name<script>', details: '<b>bold</b>', location: { coordinates: [1, 2], type: 'Point' } };
        const out = s.sanitizePoint(poi);
        expect(out.href).not.toContain('<script>');
        expect(out.name).not.toContain('<script>');
        expect(out.details).toContain('&lt;b&gt;bold&lt;/b&gt;');
    });

    it('cleans category to safe characters and defaults to other', () => {
        const poi: PointOfInterest = { href: '', category: '<img/>', name: 'name', details: 'x', location: { coordinates: [0, 0], type: 'Point' } };
        const out = s.sanitizePoint(poi);
        expect(out.category).toBe('other');

        const poi2: PointOfInterest = { href: '', category: 'My Category!', name: 'name', details: 'x', location: { coordinates: [0, 0], type: 'Point' } };
        const out2 = s.sanitizePoint(poi2);
        expect(out2.category).toBe('other');
    });

    it('keeps newline whitespace', () => {
        const poi: PointOfInterest = { href: '', category: 'cat', name: 'name', details: `  first line\nsecond line    `, location: { coordinates: [0, 0], type: 'Point' } };
        const out = s.sanitizePoint(poi);
        expect(out.details.length).toBeLessThanOrEqual(2000);
        expect(out.details).toContain('first line\nsecond line');
    });

    it('collapses non newline whitespace and limits length', () => {
        const long = 'a'.repeat(3000);
        const poi: PointOfInterest = { href: '', category: 'cat', name: 'name', details: `  multiple\n\tspaces  ${long}`, location: { coordinates: [0, 0], type: 'Point' } };
        const out = s.sanitizePoint(poi);
        expect(out.details.length).toBeLessThanOrEqual(2000);
        expect(out.details).not.toContain('\t');
    });


    it('normalizes coordinates to numbers', () => {
        const poi: any = { href: '', category: 'cat', details: '', location: { coordinates: ['13.7', '51.05'], type: 'Point' } };
        const out = s.sanitizePoint(poi);
        expect(out.location.coordinates[0]).toBeCloseTo(13.7, 5);
        expect(out.location.coordinates[1]).toBeCloseTo(51.05, 5);
    });
});
