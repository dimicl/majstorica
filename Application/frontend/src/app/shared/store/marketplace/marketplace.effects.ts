import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, from } from 'rxjs';
import { JobService } from '../../services/job.service';
import { MarketplaceActions } from './marketplace.actions';

@Injectable()
export class MarketplaceEffects {
  private actions$ = inject(Actions);
  private jobService = inject(JobService);

  loadJobs$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MarketplaceActions.loadJobs),
      switchMap(({ page, pageSize }) =>
        from(this.jobService.getMarketplaceJobs(page, pageSize)).pipe(
          map((jobs) =>
            MarketplaceActions.loadJobsSuccess({
              jobs,
              page,
              pageSize,
              hasMore: jobs.length === pageSize,
            })
          ),
          catchError((error) =>
            of(
              MarketplaceActions.loadJobsFailure({
                error:
                  (error?.status === 404
                    ? 'Marketplace endpoint nije dostupan. Pokreni/restartuj backend.'
                    : null) ??
                  error?.error?.message ??
                  error?.message ??
                  'Nije moguće učitati marketplace poslove.',
              })
            )
          )
        )
      )
    )
  );
}
