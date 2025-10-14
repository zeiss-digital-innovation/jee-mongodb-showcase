import { Injectable } from "@angular/core";
import { PointOfInterest } from "../model/point_of_interest";
import { sanitizeCategory, DEFAULT_POI_CATEGORY } from "../model/poi-categories";

@Injectable({
    providedIn: "root"
})
export class Sanitizer {
    // Basic client-side sanitization to reduce risk of sending harmful text to the backend or
    // having it rendered unsafely elsewhere. This is not a substitute for server-side validation.
    sanitizePoint(point: PointOfInterest): PointOfInterest {
        const maxDetails = 2000;

        const sanitized: PointOfInterest = {
            href: this.sanitizeText(point.href || '', 200),
            category: sanitizeCategory(point.category) as any || DEFAULT_POI_CATEGORY,
            details: this.sanitizeText(point.details || '', maxDetails),
            location: {
                type: (point.location && point.location.type) ? point.location.type : 'Point',
                coordinates: Array.isArray(point.location?.coordinates) ? [
                    Number(point.location!.coordinates[0]) || 0,
                    Number(point.location!.coordinates[1]) || 0
                ] as [number, number] : [0, 0]
            }
        };

        return sanitized;
    }

    // Escape HTML special characters and collapse whitespace
    sanitizeText(s: string | undefined, maxLen: number): string {
        if (!s) return '';
        let t = s.trim();
        // collapse whitespace
        t = t.replace(/\s+/g, ' ');
        // limit length
        if (t.length > maxLen) t = t.substring(0, maxLen);
        // escape HTML special chars to avoid accidental rendering later
        t = t.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
        return t;
    };
}