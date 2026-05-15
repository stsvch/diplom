// auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Guard закрывает защищённые маршруты: если сессии нет ни в signal, ни в localStorage, пользователь уходит на login.
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() || authService.ensureSessionRestored()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
