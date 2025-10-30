// Central list of POI categories shared across the app.
// Keep this file minimal and immutable so other modules can import stable data.

export const POI_CATEGORIES = [
    'cash',
    'coffee',
    'company',
    'gasstation',
    'lodging',
    'parking',
    'pharmacy',
    'police',
    'post',
    'restaurant',
    'supermarket',
    'toilet',
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

export function getBootstrapIconClass(category: string | undefined) {
    const cat = (category || '').toLowerCase();

    const iconClassDefault = 'bi-geo-alt';

    let iconClass = '';

    switch (cat) {
        case 'cash':
            iconClass = 'bi-credit-card';
            break;
        case 'coffee':
            iconClass = 'bi-cup-hot';
            break;
        case 'company':
            iconClass = `bi-building`;
            break;
        case 'gasstation':
            iconClass = `bi-fuel-pump`;
            break;
        case 'lodging':
            iconClass = `bi-house`;
            break;
        case 'parking':
            iconClass = `bi-car-front`;
            break;
        case 'pharmacy':
            iconClass = `bi-plus-square`;
            break;
        case 'police':
            iconClass = `bi-shield-check`;
            break;
        case 'post':
            iconClass = `bi-mailbox`;
            break;
        case 'restaurant':
            iconClass = `bi-fork-knife`;
            break;
        case 'supermarket':
            iconClass = `bi-shop`;
            break;
        case 'toilet':
            iconClass = `bi-person-standing`;
            break;
        default:
            iconClass = iconClassDefault;
    }

    return iconClass;
}

