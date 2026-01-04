import { ApplicationConfig, provideZoneChangeDetection, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideAngularSvgIcon } from 'angular-svg-icon';

import { routes } from './app.routes';

// Reducers
import { authReducer } from './shared/store/auth/auth.reducer';

// Effects
import { AuthEffects } from './shared/store/auth/auth.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withFetch()),
    
    // Angular SVG Icon
    provideAngularSvgIcon(),

    // NgRx Store
    provideStore({
      auth: authReducer,
      // Dodaj ostale reducere ovde
    }),

    // NgRx Effects
    provideEffects([
      AuthEffects,
      // Dodaj ostale effects ovde
    ]),

    // NgRx DevTools - samo u development modu
    provideStoreDevtools({
      maxAge: 25, // Pamti poslednjih 25 akcija
      logOnly: !isDevMode(), // U produkciji samo logovanje
      autoPause: true, // Pauzira kada DevTools nije otvoren
      trace: false,
      traceLimit: 75,
    }),
  ],
};
