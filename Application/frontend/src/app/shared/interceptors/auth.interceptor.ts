import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpStatusCode,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const API_BASE = 'http://localhost:5187/api';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  const isApiRequest = req.url.startsWith(API_BASE);
  if (isApiRequest && token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === HttpStatusCode.Unauthorized && isApiRequest) {
        authService.removeToken();
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
