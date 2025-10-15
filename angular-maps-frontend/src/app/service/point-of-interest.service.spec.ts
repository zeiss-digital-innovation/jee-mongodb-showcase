import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PointOfInterestService } from './point-of-interest.service';
import { PointOfInterest } from '../model/point_of_interest';

describe('PointOfInterestService sanitization', () => {
    let service: PointOfInterestService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [PointOfInterestService]
        });

        service = TestBed.inject(PointOfInterestService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should sanitize category and details before POST', () => {
        const malicious: PointOfInterest = {
            href: 'http://example.com',
            category: '<script>alert(1)</script>!@#',
            details: 'Nice place\n<script>alert(1)</script>',
            location: { coordinates: [13.73, 51.05], type: 'Point' }
        };

        service.createPointOfInterest(malicious).subscribe();

        const req = httpMock.expectOne('/poi');
        expect(req.request.method).toBe('POST');

        const body: PointOfInterest = req.request.body;
        expect(body.category).not.toContain('<script>');
        expect(body.category).toMatch(/^[\w\s\-_]+$/);
        expect(body.details).not.toContain('<script>');
        req.flush({ ...body, href: 'http://backend/poi/1' });
    });
});
