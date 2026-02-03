import { createReducer, on } from '@ngrx/store';
import { AuthActions } from '../auth/auth.actions';
import { MasterActions } from './master.actions';
import { MasterState, initialMasterState } from './master.state';

export const masterReducer = createReducer<MasterState>(
  initialMasterState,

  on(MasterActions.loadProfile, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(MasterActions.loadProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    loading: false,
  })),

  on(MasterActions.loadProfileFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AuthActions.logoutSuccess, () => initialMasterState)
);
