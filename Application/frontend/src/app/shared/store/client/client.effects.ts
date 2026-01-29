import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of } from 'rxjs';
import { ClientActions } from './client.actions';
import { ClientService } from '../../services/client.service';

@Injectable()
export class ClientEffects {
  private actions$ = inject(Actions);
  private clientService = inject(ClientService);

  loadProfile$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ClientActions.loadProfile),
      exhaustMap(() =>
        this.clientService.getClient().pipe(
          map((profile) => ClientActions.loadProfileSuccess({ profile })),
          catchError((error) =>
            of(
              ClientActions.loadProfileFailure({
                error: error.error?.message || 'Greška pri učitavanju klijenta',
              })
            )
          )
        )
      )
    )
  );
}
