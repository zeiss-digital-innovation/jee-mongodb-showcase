import { Sanitizer } from '../util/sanitization.util';
import { PoiFormatService } from './poi-format.service';

describe('PoiFormatService', () => {
    let service: PoiFormatService;
    let sanitizer: Sanitizer;

    beforeEach(() => {
        sanitizer = new Sanitizer();
        service = new PoiFormatService(sanitizer);
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
});
