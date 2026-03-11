import { Injectable, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import {
  selectUser,
  selectIsAuthenticated,
  selectAuthLoading,
  selectAuthError,
  AuthActions,
} from '../store';
import { LoginRequest, RegisterRequest } from '../interfaces';

/**
 * Auth Selector Service
 *
 * Servis za pristup podacima iz auth state-a.
 * Komponente koriste ovaj servis da čitaju podatke umesto direktnog pristupa store-u.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthSelectorService {
  private store = inject(Store);

  readonly userSelector$: Observable<User | null> =
    this.store.select(selectUser);

  readonly isAuthenticated$: Observable<boolean> = this.store.select(
    selectIsAuthenticated
  );

  readonly loading$: Observable<boolean> = this.store.select(selectAuthLoading);

  readonly error$: Observable<string | null> =
    this.store.select(selectAuthError);

  /**
    Dispatch actions
  */

  readonly dispatchLogin = (request: LoginRequest) => {
    this.store.dispatch(AuthActions.login({ request }));
  };

  readonly dispatchRegister = (request: RegisterRequest) => {
    this.store.dispatch(AuthActions.register({ request }));
  };

  readonly dispatchLogout = () => {
    this.store.dispatch(AuthActions.logout());
  };

  readonly dispatchLoadUser = () => {
    this.store.dispatch(AuthActions.loadUser());
  };

  readonly dispatchClearError = () => {
    this.store.dispatch(AuthActions.clearError());
  };
}
