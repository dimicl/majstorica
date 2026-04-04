import { createReducer, on } from '@ngrx/store';
import { AuthActions } from '../auth/auth.actions';
import { MarketplaceActions } from './marketplace.actions';
import {
  MarketplaceState,
  initialMarketplaceState,
} from './marketplace.state';

export const marketplaceReducer = createReducer<MarketplaceState>(
  initialMarketplaceState,

  on(MarketplaceActions.loadJobs, (state, { page, pageSize }) => ({
    ...state,
    page,
    pageSize,
    loading: true,
    error: null,
    jobs: page === 1 ? [] : state.jobs,
  })),

  on(
    MarketplaceActions.loadJobsSuccess,
    (state, { jobs, page, pageSize, hasMore }) => ({
      ...state,
      jobs:
        page === 1
          ? jobs
          : [
              ...state.jobs,
              ...jobs.filter(
                (next) => !state.jobs.some((existing) => existing.jobId === next.jobId)
              ),
            ],
      page,
      pageSize,
      hasMore,
      loading: false,
      error: null,
    })
  ),

  on(MarketplaceActions.loadJobsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AuthActions.logoutSuccess, () => initialMarketplaceState)
);
