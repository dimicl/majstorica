import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthSelectorService } from '../services/auth-selector.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthSelectorService);
  const router = inject(Router);

  return auth.isAuthenticated$.pipe(
    take(1),
    map((isAuthenticated) => {
      if (isAuthenticated) {
        return true;
      }

      router.navigate(['/login'], {
        queryParams: { returnUrl: state.url },
      });
      return false;
    })
  );
};
