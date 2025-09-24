import { Routes } from '@angular/router';
import { PointOfInterestListComponent } from './point-of-interest-list/point-of-interest-list.component';
import { PointOfInterestMapComponent } from './point-of-interest-map/point-of-interest-map.component';


export const routes: Routes = [
    { path: 'poi', component: PointOfInterestListComponent },
    { path: '', redirectTo: '/map', pathMatch: 'full' },
    { path: 'map', component: PointOfInterestMapComponent }
];
