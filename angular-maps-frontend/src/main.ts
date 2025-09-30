import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { BaseUrlInterceptor } from './app/interceptor/base-url.interceptor';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, {
  providers: [
    // enable DI-provided interceptors
    provideHttpClient(
      withInterceptorsFromDi()
    ),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BaseUrlInterceptor,
      multi: true
    },
    ...appConfig.providers || []
  ]
})
  .catch((err) => console.error(err));
