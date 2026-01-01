import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, exhaustMap, of, tap } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { AuthActions } from './auth.actions';

@Injectable()
export class AuthEffects {
  private actions$ = inject(Actions);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Login Effect - poziva API i vraća success ili failure
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ email, password }) =>
        this.authService.login(email, password).pipe(
          map((response) => {
            this.authService.saveToken(response.token);
            return AuthActions.loginSuccess({ user: response.user });
          }),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error.error?.message || 'Greška pri prijavi' }))
          )
        )
      )
    )
  );

  // Nakon uspešnog logina - redirect na home
  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(() => {
          this.router.navigate(['/']);
        })
      ),
    { dispatch: false }
  );

  // Register Effect
  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.register),
      exhaustMap(({ email, password, name }) =>
        this.authService.register(email, password, name).pipe(
          map((response) => {
            this.authService.saveToken(response.token);
            return AuthActions.registerSuccess({ user: response.user });
          }),
          catchError((error) =>
            of(AuthActions.registerFailure({ error: error.error?.message || 'Greška pri registraciji' }))
          )
        )
      )
    )
  );

  // Nakon uspešne registracije - redirect na home
  registerSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.registerSuccess),
        tap(() => {
          this.router.navigate(['/']);
        })
      ),
    { dispatch: false }
  );

  // Logout Effect
  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      exhaustMap(() =>
        this.authService.logout().pipe(
          map(() => {
            this.authService.removeToken();
            return AuthActions.logoutSuccess();
          }),
          catchError(() => {
            // Čak i ako API poziv ne uspe, obriši token lokalno
            this.authService.removeToken();
            return of(AuthActions.logoutSuccess());
          })
        )
      )
    )
  );

  // Nakon logouta - redirect na login
  logoutSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess),
        tap(() => {
          this.router.navigate(['/login']);
        })
      ),
    { dispatch: false }
  );
}
