import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of } from 'rxjs';
import { MasterActions } from './master.actions';
import { MasterService } from '../../services/master.service';

@Injectable()
export class MasterEffects {
  private actions$ = inject(Actions);
  private masterService = inject(MasterService);

  loadProfile$ = createEffect(() =>
    this.actions$.pipe(
      ofType(MasterActions.loadProfile),
      exhaustMap(() =>
        this.masterService.getMaster().pipe(
          map((profile) => MasterActions.loadProfileSuccess({ profile })),
          catchError((error) =>
            of(
              MasterActions.loadProfileFailure({
                error: error.error?.message || 'Greška pri učitavanju majstora',
              })
            )
          )
        )
      )
    )
  );
}
