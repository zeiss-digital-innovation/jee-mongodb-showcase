import { Pipe, PipeTransform } from '@angular/core';
import { PoiFormatService } from '../service/poi-format.service';

@Pipe({ name: 'formatDetails', standalone: true })
export class FormatDetailsPipe implements PipeTransform {
    constructor(private poiFormat: PoiFormatService) { }

    transform(value: string | undefined): string {
        return this.poiFormat.formatDetails(value);
    }
}

