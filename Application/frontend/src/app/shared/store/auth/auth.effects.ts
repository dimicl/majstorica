import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, exhaustMap, of, tap } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { SignalrService } from '../../services/signalr.service';
import { ChatService } from '../../services/chat.service';
import { AuthActions } from './auth.actions';
import { HUB_CHAT_URL } from '../../constants/api.constants';

@Injectable()
export class AuthEffects {
  private actions$ = inject(Actions);
  private authService = inject(AuthService);
  private signalr = inject(SignalrService);
  private chatService = inject(ChatService);
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

  // Nakon uspešnog logina: user u auth state, konekcija na SignalR, redirect na home
  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(() => {
          void this.signalr
            .connect(HUB_CHAT_URL, {
              accessTokenFactory: () => this.authService.getToken() ?? '',
            })
            .then(() => this.chatService.registerRealtimeHandlers());
          this.router.navigate(['/home']);
        })
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

  // Nakon uspešne registracije: user u auth state, konekcija na SignalR, redirect na home
  registerSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.registerSuccess),
        tap(() => {
          void this.signalr
            .connect(HUB_CHAT_URL, {
              accessTokenFactory: () => this.authService.getToken() ?? '',
            })
            .then(() => this.chatService.registerRealtimeHandlers());
          this.router.navigate(['/home']);
        })
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

  // Logout Success - prekid SignalR konekcije i redirect na login
  logoutSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess),
        tap(() => {
          this.chatService.clearRealtimeHandlers();
          void this.signalr.disconnect();
          this.router.navigate(['/login']);
        })
      ),
    { dispatch: false }
  );

  saveUserIdToStorage$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(
          AuthActions.loginSuccess,
          AuthActions.registerSuccess,
          AuthActions.loadUserSuccess
        ),
        tap((action) => {
          const { user } = action;
          if (user?.id) this.authService.saveUserId(user.id);
        })
      ),
    { dispatch: false }
  );

  // Kada je korisnik već ulogovan (npr. osvežio stranicu) – konektuj SignalR da ostaneš online
  loadUserSuccessConnect$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loadUserSuccess),
        tap(() => {
          void this.signalr
            .connect(HUB_CHAT_URL, {
              accessTokenFactory: () => this.authService.getToken() ?? '',
            })
            .then(() => this.chatService.registerRealtimeHandlers());
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
        const userId = this.authService.getUserIdFromStorage();
        if (!token || !userId) {
          return of(AuthActions.loadUserFailure());
        }

        return this.authService.getUserById(userId).pipe(
          map((response) =>
            AuthActions.loadUserSuccess({
              user: response.user,
              token,
            })
          ),
          catchError(() => of(AuthActions.loadUserFailure()))
        );
      })
    )
  );
}
