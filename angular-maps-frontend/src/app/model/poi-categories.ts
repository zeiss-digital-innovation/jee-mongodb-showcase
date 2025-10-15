// Central list of POI categories shared across the app.
// Keep this file minimal and immutable so other modules can import stable data.

export const POI_CATEGORIES = [
    'restaurant',
    'cafe',
    'bar',
    'park',
    'museum',
    'hotel',
    'shop',
    'library',
    'pharmacy',
    'hospital',
    'atm',
    'parking',
    'other'
] as const;

export type PoiCategory = typeof POI_CATEGORIES[number];

export const DEFAULT_POI_CATEGORY: PoiCategory = 'other';

/**
 * Returns true if the provided value is one of the known POI categories.
 */
export function isValidCategory(value: string | undefined | null): value is PoiCategory {
    if (!value) return false;
    return (POI_CATEGORIES as readonly string[]).includes(value.toLowerCase());
}

/**
 * Normalizes a category string: trims, lowercases and returns a known category or the default.
 */
export function sanitizeCategory(value: string | undefined | null): PoiCategory {
    if (!value) return DEFAULT_POI_CATEGORY;
    const v = value.trim().toLowerCase();
    return isValidCategory(v) ? (v as PoiCategory) : DEFAULT_POI_CATEGORY;
}
