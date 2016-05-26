import {bootstrap}    from '@angular/platform-browser-dynamic';
import {AppComponent} from './app.component';
import {ANGULAR2_GOOGLE_MAPS_PROVIDERS} from 'angular2-google-maps/core';

// this line boots our application on the page in the <my-app> element:
// Note: It is required to add the ANGULAR2_GOOGLE_MAPS_PROVIDERS here!
bootstrap(AppComponent, [ANGULAR2_GOOGLE_MAPS_PROVIDERS]);
