// auth.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

// Interceptor добавляет Bearer token и credentials к API-запросам, а при 401 пытается обновить access token через refresh endpoint.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isRefreshRequest = req.url.includes('/auth/refresh');
  const isAuthEndpoint = req.url.includes('/auth/');

  // Access token уходит в Authorization, а credentials нужны для refresh-cookie на auth endpoints.
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
      // При 401 вне refresh-запроса пробуем получить новый access token и повторить исходный запрос.
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
