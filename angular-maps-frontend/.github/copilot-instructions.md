# AI Coding Agent Instructions for Angular POI Frontend

## Project Overview
This is an Angular 18 standalone components frontend for Points of Interest (POI) mapping and management. It displays POIs on a Leaflet map, provides list/filter views, and supports CRUD operations with Bootstrap UI.

**Key Stack:** Angular 18 (standalone), Bootstrap 5, Leaflet, RxJS, TypeScript strict mode.

## Architecture Patterns

### Standalone Components & DI
- **All components are standalone** (no `NgModule`). They import dependencies directly.
- Services use `providedIn: 'root'` decorator for singleton injection.
- **Dynamic component creation** pattern: use `createComponent(ComponentClass, { environmentInjector: injector })` for runtime dialog/modal generation, then attach with `appRef.attachView()` and append to `document.body`.
- Example: `PoiDialogComponent` is created dynamically in `PointOfInterestListComponent.editPoi()`.

### Service Architecture
- **`PointOfInterestService`**: REST client for CRUD ops on POIs. Sanitizes input before POST/PUT via injected `Sanitizer`.
- **`MapDataService`**: Provides `zoomToRadiusMap` (Map<number, number>) + methods `mapZoomToRadius()` and `mapRadiusToZoom()` for zoom ↔ radius conversions. Also formats marker popup HTML.
- **`PoiFilterService`**: Stores/retrieves filter criteria in `BehaviorSubject`, provides `filter()` method (case-insensitive substring matching on name/details/category).
- **`Sanitizer`**: Client-side HTML escaping + URL/phone detection. **Not a substitute for server-side validation**.
- **`PoiFormatService`**: Detects URLs/phones in details, wraps in safe `<a>` tags or phone icons with HTML formatting.

### HTTP Interceptor Pattern
**`BaseUrlInterceptor`** prepends `environment.apiBaseUrl` to relative URLs. All relative requests become absolute automatically. Configure `apiBaseUrl` in `src/app/environments/environment.ts` (dev) and `environment.prod.ts` (prod).

### Zoom ↔ Radius Mapping
The `zoomToRadiusMap` in `MapDataService` is the single source of truth (zoom 9→50000m, 10→30000m, ..., 15→1000m).
- **`mapZoomToRadius(zoom)`**: Direct lookup; falls back to closest lower zoom.
- **`mapRadiusToZoom(radius)`**: Finds closest radius by min absolute difference.
- Tests verify round-trip consistency and edge cases.

## Component Responsibilities

### Two-Page Layout
- **`PointOfInterestMapComponent`** (`/map`): Leaflet map, right-click to add POI, filters at top, toast notifications.
- **`PointOfInterestListComponent`** (`/poi`): Table view, sortable columns, delete confirmation modal (programmatic Bootstrap Modal with `window.confirm` fallback), edit via dynamic dialog.

### Modal & Dialog Flows
- **Delete Modal** (List page): Uses `openDeleteConfirmationModal(poi)` → sets `selectedPoi` → dynamic import Bootstrap Modal → `show()` → `confirmDeleteSelectedPoi()` calls `deletePoi()` and hides modal. Fallback: `window.confirm()`.
- **Add/Edit Dialog**: Dynamic `PoiDialogComponent` created with `createComponent()`, attached to `document.body`, emits `save` or `cancel` events.
- **Toast Notifications**: Dynamically imported Bootstrap Toast via `ngAfterViewInit`. Fallback: `window.bootstrap.Toast` (global bundle). Retry logic (6 attempts, 50ms delay) if not immediately ready.

## Testing Patterns

### TestBed Setup
Always import testing modules for dependencies:
- **HTTP**: Add `HttpClientTestingModule` to TestBed imports.
- **Routing**: Add `RouterTestingModule` for any route-aware components.
- **DOM**: Leaflet components need `#map` div in DOM before `fixture.detectChanges()`.

Example:
```typescript
TestBed.configureTestingModule({
  imports: [ComponentClass, HttpClientTestingModule, RouterTestingModule]
});
```

### Service Tests
- Inject services directly; use `HttpTestingController` for HTTP mocking.
- Example: `map-data.service.spec.ts` tests `mapZoomToRadius()` and `mapRadiusToZoom()` with exact and edge-case values.

## Security & Sanitization

### XSS Prevention
1. **Client-side**: `Sanitizer` class escapes HTML entities and limits text length (e.g., `maxText: 2000`).
2. **Data Flow**: `PointOfInterestService` sanitizes POIs before POST/PUT via `sanitizer.sanitizePoint()`.
3. **Rendering**: Use `[innerHTML]="formatDetails()"  pipe (safe because `PoiFormatService` escapes non-URLs and validates `isSafeUrl()`).
4. **Critical**: Server-side validation is authoritative; client-side is a defense layer only.

### URL & Phone Patterns
- `isSafeUrl()` whitelist: `http://`, `https://`, `www.`
- Phone detection: `+49` prefix or `Tel.:` prefix (escaped and formatted with icon).

## Build & Dev Workflow

### Key Commands
```bash
npm install                    # Install deps
ng serve                       # Dev server (localhost:4200)
npm test                       # Unit tests (watch mode)
npm test -- --watch=false --browsers=ChromeHeadless  # CI-friendly
ng build --configuration production  # Prod build → dist/
```

### File Organization
```
src/
  app/
    service/          # Singleton services (MapDataService, PoiFilterService, etc.)
    model/            # Data types (PointOfInterest, ToastNotification, poi-categories)
    util/             # Sanitizer utility
    environments/     # environment.ts (dev), environment.prod.ts
    interceptor/      # BaseUrlInterceptor
    pipe/             # FormatDetailsPipe
    poi-dialog/       # Dynamic dialog component
    point-of-interest-map/    # Map page
    point-of-interest-list/   # List page (with delete modal)
  styles.css          # Global CSS (Bootstrap overrides, .toast-fixed, nav selected state)
  typings/            # bootstrap-modal.d.ts, bootstrap-toast.d.ts (module declarations)
public/media/leaflet/ # Leaflet assets (marker icons, tiles)
```

### Styling Notes
- **CSS Variables**: `:root` defines theme colors (`--color-azure`, `--color-indigo`, etc.).
- **Bootstrap Overrides**: Applied after Bootstrap import in `angular.json` styles order.
- **Fixed Controls**: `.top-controls` uses `--navbar-height` + `--top-controls-height` variables; responsive media queries adjust heights (170px mobile, 260px tablet).
- **Sticky Headers**: `.table thead th { position: sticky; top: 0; }` keeps table headers visible on scroll.

## Common Tasks

### Add a Filter
1. Add property to component (e.g., `myFilter: string | undefined`).
2. Add method to handle user input (e.g., `filterByMyField(event: Event)`).
3. Pass to `poiFilterService.filter()` method.
4. Update `PoiFilterCriteria` interface if persisting state.

### Add a New Toast
Use `showToastMessage(title, message, smallMessage, cssClass)` from any component with `@ViewChild('messageToast')` and `ngAfterViewInit` toast initialization. Bootstrap Toast is auto-loaded via dynamic import.

### Extend Zoom/Radius Map
1. Update `zoomToRadiusMap` in `MapDataService` constructor.
2. Add corresponding test cases in `map-data.service.spec.ts` for exact and edge-case values.
3. Tests verify both `mapZoomToRadius()` and `mapRadiusToZoom()`.

### Add Dynamic Component
Use the `PoiDialogComponent` pattern:
```typescript
const compRef = createComponent(MyComponent, { environmentInjector: this.injector });
compRef.instance.myInput = value;
this.appRef.attachView(compRef.hostView);
document.body.appendChild(compRef.location.nativeElement);
// Subscribe to outputs for cleanup
compRef.instance.myEvent.subscribe(() => { /* cleanup */ });
```

## Known Patterns & Quirks

- **Dynamic Imports with Fallback**: Bootstrap modules (Modal, Toast) are imported dynamically; TypeScript needs ambient declarations in `src/typings/` to avoid "Cannot find module" errors.
- **Modal & Toasts Behind Map**: Fixed to `document.body` with high `z-index` (10750) to appear above Leaflet overlays.
- **Change Detection**: Use `this.cd.detectChanges()` after setting `selectedPoi` in modal flows to ensure bindings sync before modal.show().
- **Leaflet Right-Click**: Handled via `map.on('contextmenu', ...)` + `event.originalEvent.preventDefault()` + native `contextmenu` listener on container.
- **Typescript Strict Mode**: Strict null checks enabled; use non-null assertion (`!`) only after safe checks (e.g., `map!.getZoom()`).

## AI Agent Checklist

- [ ] Imports: Standalone components, `HttpClientTestingModule` in tests, DI via `providedIn: 'root'`.
- [ ] Sanitization: Use `Sanitizer.sanitizeText()` + `isSafeUrl()` before rendering user input.
- [ ] Tests: Cover happy path + edge cases; use mocks (`HttpTestingController`, `spyOn`).
- [ ] Styling: CSS variables in `:root`, responsive via media queries, `.toast-fixed` for overlays.
- [ ] Zoom/Radius: Consult `zoomToRadiusMap` in `MapDataService` as canonical source.
- [ ] Environments: Update `apiBaseUrl` in both `environment.ts` files; interceptor applies globally.
