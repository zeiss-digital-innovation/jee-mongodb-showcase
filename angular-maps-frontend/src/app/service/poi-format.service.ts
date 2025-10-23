import { Injectable } from '@angular/core';
import { Sanitizer } from '../util/sanitization.util';

@Injectable({ providedIn: 'root' })
export class PoiFormatService {

    public iconPhone = `<i class="bi bi-telephone"></i>`;
    public iconLink = `<i class="bi bi-link-45deg"></i>`;

    constructor(private sanitizer: Sanitizer) { }

    formatDetails(details: string | undefined): string {
        if (!details) return '';

        const detailChunks = details.split(/, |\n/).map(s => (s || '').trim()).filter(s => s.length > 0);

        const formatted = detailChunks.map(chunk => {
            if (this.sanitizer.isSafeUrl(chunk)) {
                return this.formatForLink(chunk);
            }
            return this.formatForPhone(chunk);
        });

        return formatted.join('<br>');
    }

    formatForLink(text: string): string {
        const t = (text || '').trim();
        if (this.sanitizer.isSafeUrl(t)) {
            const href = t.toLowerCase().startsWith('http') ? t : `https://${t}`;
            const sanitizedHref = this.sanitizer.sanitizeText(href, this.sanitizer.maxHref);
            return this.iconLink + ` <a href="${sanitizedHref}" target="_blank" rel="noopener">${sanitizedHref}</a>`;
        }
        return this.sanitizer.sanitizeText(t, this.sanitizer.maxText);
    }

    formatForPhone(text: string): string {
        const raw = (text || '').trim();
        if (!raw) return '';

        if (raw.startsWith('+49')) {
            return this.iconPhone + ' ' + this.sanitizer.sanitizeText(raw, this.sanitizer.maxPhone);
        }

        if (raw.startsWith('Tel.:')) {
            const num = raw.replace('Tel.:', '').trim();
            return this.iconPhone + ' ' + this.sanitizer.sanitizeText(num, this.sanitizer.maxPhone);
        }

        return this.sanitizer.sanitizeText(raw, this.sanitizer.maxText);
    }
}
