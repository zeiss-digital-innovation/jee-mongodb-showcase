export class PointOfInterest {
    href: string;
    category: string;
    details: string;

    location: {
        coordinates: [number, number];
        type: string;
    };

    constructor(href: string, category: string, details: string, location: { coordinates: [number, number]; type: string }) {
        this.href = href;
        this.category = category;
        this.details = details;
        this.location = location;
    }

    static createEmptyFromCoordinates(latitude: number, longitude: number): PointOfInterest {
        return new PointOfInterest('', '', '', { coordinates: [longitude, latitude], type: 'Point' });
    }
}