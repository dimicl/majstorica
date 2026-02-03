import { createReducer, on } from '@ngrx/store';
import { AuthActions } from '../auth/auth.actions';
import { ClientActions } from './client.actions';
import { initialClientState } from './client.state';

export const clientReducer = createReducer(
  initialClientState,

  on(ClientActions.loadProfile, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(ClientActions.loadProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    loading: false,
  })),

  on(ClientActions.loadProfileFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AuthActions.logoutSuccess, () => initialClientState)
);
