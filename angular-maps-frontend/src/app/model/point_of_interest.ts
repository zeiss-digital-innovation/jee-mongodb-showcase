export interface PointOfInterest {
    href: string;
    category: string;
    details: string;

    location: {
        coordinates: [number, number];
        type: string;
    };
}