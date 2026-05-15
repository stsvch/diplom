// error.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { parseApiError } from '../models/api-error.model';

// Interceptor приводит ошибки backend к единому ApiError, чтобы компоненты показывали пользователю понятный текст.
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError = parseApiError(error);
      return throwError(() => apiError);
    }),
  );
};
