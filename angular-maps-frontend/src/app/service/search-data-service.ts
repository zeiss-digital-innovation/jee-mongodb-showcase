import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { environment } from '../environments/environment';

export interface SearchData { latitude: number; longitude: number; radius: number; }

@Injectable({
    providedIn: 'root'
})
export class SearchDataService {

    private searchData$ = new BehaviorSubject<SearchData | null>(null);

    latitudeDefault: number;
    longitudeDefault: number;
    radiusDefault: number;

    constructor() {
        this.latitudeDefault = environment.latitudeDefault;
        this.longitudeDefault = environment.longitudeDefault;
        this.radiusDefault = environment.radiusDefault;
    }

    ngOnInit(): void {
        this.searchData$.next({ latitude: this.latitudeDefault, longitude: this.longitudeDefault, radius: this.radiusDefault });
    }

    setSearchData(data: SearchData) {
        this.searchData$.next(data);
    }

    getSearchData(): SearchData | null {
        return this.searchData$.getValue();
    }

    clear() {
        this.searchData$.next(null);
    }
}