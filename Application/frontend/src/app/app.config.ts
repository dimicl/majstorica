import { ApplicationConfig, provideZoneChangeDetection, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideAngularSvgIcon } from 'angular-svg-icon';

import { routes } from './app.routes';
import { authInterceptor } from './shared/interceptors/auth.interceptor';

// Reducers
import { authReducer } from './shared/store/auth/auth.reducer';
import { clientReducer } from './shared/store/client/client.reducer';
import { masterReducer } from './shared/store/master/master.reducer';

// Effects
import { AuthEffects } from './shared/store/auth/auth.effects';
import { ClientEffects } from './shared/store/client/client.effects';
import { MasterEffects } from './shared/store/master/master.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    
    // Angular SVG Icon
    provideAngularSvgIcon(),

    // NgRx Store
    provideStore({
      auth: authReducer,
      client: clientReducer,
      master: masterReducer,
    }),

    // NgRx Effects
    provideEffects([AuthEffects, ClientEffects, MasterEffects]),

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
