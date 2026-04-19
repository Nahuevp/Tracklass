import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideToastr } from "ngx-toastr"
import { provideAnimations } from '@angular/platform-browser/animations';
import { authInterceptor } from './interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    provideToastr({
      positionClass: 'toast-top-right',
      progressBar: true,
      closeButton: false,
      easeTime: 250,
      timeOut: 2200,
      preventDuplicates: true,
      newestOnTop: true,
      tapToDismiss: true,
    }),
    provideAnimations(),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes)
  ]
};
