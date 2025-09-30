import { Injectable } from '@angular/core';
import {
    HttpInterceptor,
    HttpRequest,
    HttpHandler,
    HttpEvent
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

/**
 * HTTP interceptor to prepend the base URL to all outgoing requests
 * and optionally add common headers (e.g. for JSON content-type).
 * 
 * @author AI generated
 */
@Injectable()
export class BaseUrlInterceptor implements HttpInterceptor {
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let url = req.url;

        // If URL is not absolute, prepend base URL
        if (!/^https?:\/\//i.test(url)) {
            const base = environment.apiBaseUrl.replace(/\/$/, ''); // remove trailing slash
            url = base + '/' + url.replace(/^\//, ''); // ensure single slash between
            req = req.clone({ url });
            // uncomment next line to log the rewritten URL for debugging
            // console.log('[BaseUrlInterceptor] rewritten url:', req.url);
        }

        // Optionally add common headers (e.g. JSON content-type)
        const modified = req.clone({
            setHeaders: {
                'Content-Type': 'application/json'
                // 'Authorization': `Bearer ${token}` // add auth if needed
            }
        });

        return next.handle(modified);
    }
}