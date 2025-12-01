[![Angular](https://img.shields.io/badge/Angular-21.0.1-DD0031?logo=angular&logoColor=white)](https://angular.io/)
[![Node.js](https://img.shields.io/badge/Node.js-18.x-green?logo=node.js&logoColor=white)](https://nodejs.org/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE.md)

# Angular Frontend

Small Angular frontend for the POI / map showcase.

## Table of Contents

- [Requirements](#requirements)
- [Quickstart](#quickstart)
- [Build](#build)
- [Tests](#tests)
- [Workspace notes](#workspace-notes)
- [Security / Input handling](#security--input-handling)
- [Development tips](#development-tips)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

## Requirements

- [Node.js](https://nodejs.org/) (recommended LTS, e.g. 18.x or later)
- npm (comes with Node) or yarn
- Angular CLI, install for instance using npm:
```bash
npm install -g @angular/cli
```
- Chrome (for running Karma tests) or install a compatible headless Chromium for CI

This project was generated with Angular CLI 18.2.8.

## Quickstart

1. Install dependencies
   - npm:
     npm install
   - or yarn:
     yarn install

2. Configure API base URL
   - Edit `src/app/environments/environment.ts` and `src/app/environments/environment.prod.ts` and set `apiBaseUrl` to your backend API (e.g. `http://localhost:8080/api`).

3. Start dev server (Windows)
   ng serve
   - Open http://localhost:4200

## Build

- Development build:
  ng build
- Production build:
  ng build --configuration production

Built artifacts end up in `dist/`.

## Tests

- Run unit tests (Karma):
  npm test
  or
  ng test

- Headless (useful for CI / when Chrome GUI is not available):
  ng test --watch=false --browsers=ChromeHeadless

If Karma fails with Chrome disconnects on CI, configure a headless launcher with no-sandbox flags in `karma.conf.js` (e.g. `ChromeHeadlessNoSandbox`) or use `karma-chrome-launcher` with appropriate flags.

Common test fixes:
- If a spec reports "No provider for HttpClient!" add `HttpClientTestingModule` to the TestBed imports for that spec.
- If a spec reports "No provider for ActivatedRoute!" add `RouterTestingModule` to TestBed imports.
- For components using DOM libraries (Leaflet), create minimal DOM elements the component expects (e.g. `#map`) before `fixture.detectChanges()`.

## Workspace notes

- CSS: [Bootstrap](https://getbootstrap.com/) + [Bootstrap Icons](https://icons.getbootstrap.com/) is used for styling. Neccessary css / js files will be available after loading dependencies.
- Maps: [Leaflet](https://leafletjs.com/) is used; its assets (marker icons, tiles) can be found in `public/media/leaflet/`.
- Runtime backend base URL: this project uses an HttpInterceptor to prefix relative URLs with `environment.apiBaseUrl`. Keep `environment.*.ts` values accurate.

## Security / Input handling

- The app performs client-side sanitization but server-side validation is required and must be authoritative.
- If any pipe or service returns HTML for `[innerHTML]`, validate and/or use Angular `DomSanitizer` carefully.

## Development tips

- Use standalone components and import small dependencies per-component to keep bundles small.
- For dynamic components created with `createComponent(...)` pass `environmentInjector` so DI providers are available.
- For dialogs created at runtime, attach/detach views with `ApplicationRef.attachView(...)` or use `ViewContainerRef` to manage lifecycle.

## Troubleshooting

- Dev server not seeing changes: stop `ng serve` and restart it.
- Map right-click triggers browser context menu: handle Leaflet's `contextmenu` event and call `event.originalEvent.preventDefault()`, plus add a native `contextmenu` listener on the map container for robustness.
- Tests failing due to missing providers: update TestBed imports/providers as described above.

## Contributing

- Run tests locally before pushing.
- Keep unit tests focused and add tests for services that handle sanitization/formatting.
