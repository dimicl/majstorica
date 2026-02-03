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
      exhaustMap(({ request }) =>
        this.authService.login(request).pipe(
          map((response) => {
            this.authService.saveToken(response.token);
            return AuthActions.loginSuccess({
              user: response.user,
              token: response.token,
            });
          }),
          catchError((error) =>
            of(
              AuthActions.loginFailure({
                error: error.error?.message || 'Greška pri prijavi',
              })
            )
          )
        )
      )
    )
  );

  // Nakon uspešnog logina: user u auth state, redirect na home (home će po user.id/role dispatch-ovati get)
  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(() => this.router.navigate(['/home']))
      ),
    { dispatch: false }
  );

  // Register Effect
  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.register),
      exhaustMap(({ request }) =>
        this.authService.register(request).pipe(
          map((response) => {
            this.authService.saveToken(response.token);
            return AuthActions.registerSuccess({
              user: response.user,
              token: response.token,
            });
          }),
          catchError((error) =>
            of(
              AuthActions.registerFailure({
                error: error.error?.message || 'Greška pri registraciji',
              })
            )
          )
        )
      )
    )
  );

  // Nakon uspešne registracije: user u auth state, redirect na home
  registerSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.registerSuccess),
        tap(() => this.router.navigate(['/home']))
      ),
    { dispatch: false }
  );

  // Logout
  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      map(() => {
        this.authService.removeToken();
        return AuthActions.logoutSuccess();
      })
    )
  );

  // Logout Success - redirect na login
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

  // Load User
  loadUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loadUser),
      exhaustMap(() => {
        const token = this.authService.getToken();
        if (!token) {
          return of(AuthActions.loadUserFailure());
        }

        return this.authService.getUser().pipe(
          map((response) =>
            AuthActions.loadUserSuccess({
              user: response.user,
              token,
            })
          ),
          catchError(() => {
            this.authService.removeToken();
            return of(AuthActions.loadUserFailure());
          })
        );
      })
    )
  );
}
