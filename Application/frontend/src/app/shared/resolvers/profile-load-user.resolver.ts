import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  ResolveFn,
  RouterStateSnapshot,
} from '@angular/router';
import { Store } from '@ngrx/store';
import { catchError, mapTo, of, switchMap, take } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AuthActions } from '../store/auth/auth.actions';
import { selectUser } from '../store/auth/auth.selectors';

/** Ima token (i eventualno id) → pre aktivacije stranice učitaj usera u store. */
export const profileLoadUserResolver: ResolveFn<null> = (
  _route: ActivatedRouteSnapshot,
  _state: RouterStateSnapshot
) => {
  const store = inject(Store);
  const auth = inject(AuthService);

  const token = auth.getToken();
  const id = auth.getUserIdFromStorage();
  if (!token || !id) return of(null);

  return store.select(selectUser).pipe(
    take(1),
    switchMap((user) => {
      if (user) return of(null);
      return auth.getUserById(id).pipe(
        switchMap((res) => {
          store.dispatch(
            AuthActions.loadUserSuccess({ user: res.user, token })
          );
          return of(null);
        }),
        catchError(() => of(null))
      );
    })
  );
};
