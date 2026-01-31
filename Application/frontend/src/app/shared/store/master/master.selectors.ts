import { createFeatureSelector, createSelector } from '@ngrx/store';
import { MasterState } from './master.state';

export const selectMasterState = createFeatureSelector<MasterState>('master');

export const selectMasterProfile = createSelector(
  selectMasterState,
  (state) => state.profile
);

export const selectMasterLoading = createSelector(
  selectMasterState,
  (state) => state.loading
);

export const selectMasterError = createSelector(
  selectMasterState,
  (state) => state.error
);
