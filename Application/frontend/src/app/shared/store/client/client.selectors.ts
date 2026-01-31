import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ClientState } from '../../interfaces/client-state.interface';

export const selectClientState = createFeatureSelector<ClientState>('client');

export const selectClientProfile = createSelector(
  selectClientState,
  (state) => state.profile
);

export const selectClientLoading = createSelector(
  selectClientState,
  (state) => state.loading
);

export const selectClientError = createSelector(
  selectClientState,
  (state) => state.error
);
