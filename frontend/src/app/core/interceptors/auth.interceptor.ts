import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isRefreshRequest = req.url.includes('/auth/refresh');
  const isAuthEndpoint = req.url.includes('/auth/');

  // Добавить токен и credentials
  const token = authService.getAccessToken();
  let authReq = req;

  if (token && !isRefreshRequest) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
      withCredentials: isAuthEndpoint ? true : req.withCredentials,
    });
  } else if (isAuthEndpoint) {
    authReq = req.clone({ withCredentials: true });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // При 401 (кроме refresh) — пробуем обновить токен
      if (error.status === 401 && !isRefreshRequest) {
        return authService.refreshToken().pipe(
          switchMap((res) => {
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.accessToken}` },
              withCredentials: isAuthEndpoint ? true : req.withCredentials,
            });
            return next(retryReq);
          }),
          catchError(() => {
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => error);
          }),
        );
      }
      return throwError(() => error);
    }),
  );
};
