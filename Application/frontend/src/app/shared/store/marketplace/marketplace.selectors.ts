import { createFeatureSelector, createSelector } from '@ngrx/store';
import { MarketplaceState } from './marketplace.state';

export const selectMarketplaceState =
  createFeatureSelector<MarketplaceState>('marketplace');

export const selectMarketplaceJobs = createSelector(
  selectMarketplaceState,
  (state) => state.jobs
);

export const selectMarketplaceLoading = createSelector(
  selectMarketplaceState,
  (state) => state.loading
);

export const selectMarketplaceError = createSelector(
  selectMarketplaceState,
  (state) => state.error
);

export const selectMarketplacePage = createSelector(
  selectMarketplaceState,
  (state) => state.page
);

export const selectMarketplacePageSize = createSelector(
  selectMarketplaceState,
  (state) => state.pageSize
);

export const selectMarketplaceHasMore = createSelector(
  selectMarketplaceState,
  (state) => state.hasMore
);
