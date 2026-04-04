import { createReducer, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { initialAuthState } from './auth.state';
import { User } from '../../models';

export const authReducer = createReducer(
  initialAuthState,

  // Login
  on(AuthActions.login, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(AuthActions.loginSuccess, (state, { user, token }) => ({
    ...state,
    user,
    token,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),

  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Logout
  on(AuthActions.logout, (state) => ({
    ...state,
    loading: true,
  })),

  on(AuthActions.logoutSuccess, () => initialAuthState),

  // Register
  on(AuthActions.register, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(AuthActions.registerSuccess, (state, { user, token }) => ({
    ...state,
    user,
    token,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),

  on(AuthActions.registerFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Load User (refresh – token iz localStorage, user iz API-ja)
  on(AuthActions.loadUser, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(AuthActions.loadUserSuccess, (state, { user, token }) => ({
    ...state,
    user,
    token,
    isAuthenticated: true,
    loading: false,
    error: null,
  })),
  on(AuthActions.loadUserFailure, (state) => ({
    ...state,
    loading: false,
  })),

  // Clear Error
  on(AuthActions.clearError, (state) => ({
    ...state,
    error: null,
  })),

  on(AuthActions.patchUser, (state, { partial }): typeof state => {
    if (!state.user) return state;
    const user: User = { ...state.user, ...partial };
    return { ...state, user };
  })
);
