import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthSelectorService } from '../services/auth-selector.service';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthSelectorService);
  const router = inject(Router);
  const authService = inject(AuthService);

  return auth.isAuthenticated$.pipe(
    take(1),
    map((isAuthenticated) => {
      const hasToken = !!authService.getToken();

      if (isAuthenticated || hasToken) {
        return true;
      }

      router.navigate(['/login']);
      return false;
    })
  );
};
