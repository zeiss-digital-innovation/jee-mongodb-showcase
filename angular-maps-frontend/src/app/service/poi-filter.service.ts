import { Injectable } from "@angular/core";
import { PointOfInterest } from "../model/point_of_interest";
import { BehaviorSubject } from "rxjs";

export interface PoiFilterCriteria {
    categoryFilter?: string;
    nameFilter?: string;
    detailsFilter?: string;
}

@Injectable({ providedIn: 'root' })
export class PoiFilterService {

    private filterCriteria$ = new BehaviorSubject<PoiFilterCriteria | null>(null);

    setFilterCriteria(data: PoiFilterCriteria) {
        this.filterCriteria$.next(data);
    }

    getFilterCriteria(): PoiFilterCriteria | null {
        return this.filterCriteria$.getValue();
    }

    clear() {
        this.filterCriteria$.next(null);
    }

    filter(pointsOfInterest: PointOfInterest[], categoryFilter: string | undefined, nameFilter: string | undefined, detailsFilter: string | undefined): PointOfInterest[] {
        return pointsOfInterest.filter(poi => {
            const matchesCategory = categoryFilter ? poi.category?.toLowerCase() === categoryFilter.toLowerCase() : true;
            const matchesName = nameFilter ? poi.name?.toLowerCase().includes(nameFilter.toLowerCase()) : true;
            const matchesDetails = detailsFilter ? poi.details?.toLowerCase().includes(detailsFilter.toLowerCase()) : true;
            return matchesCategory && matchesName && matchesDetails;
        });
    }

    noCategoryFilterSet(categoryFilter: string | undefined): boolean {
        return !categoryFilter || categoryFilter === undefined || categoryFilter === '' || categoryFilter === 'Choose...';
    }

    matchesCategoryCriteria(category: string | undefined, categoryFilter: string | undefined): boolean {
        return categoryFilter ? category?.toLowerCase() === categoryFilter.toLowerCase() : false;
    }
}