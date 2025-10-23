import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { environment } from '../environments/environment';

export interface SearchCriteria { latitude: number; longitude: number; radius: number; }

@Injectable({
    providedIn: 'root'
})
export class SearchCriteriaService {

    private searchData$ = new BehaviorSubject<SearchCriteria | null>(null);

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

    setSearchCriteria(data: SearchCriteria) {
        this.searchData$.next(data);
    }

    getSearchCriteria(): SearchCriteria | null {
        return this.searchData$.getValue();
    }

    clear() {
        this.searchData$.next(null);
    }
}