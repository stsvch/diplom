// role.guard.ts
import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user.model';

// Guard проверяет роль из профиля/хранилища и не пускает пользователя в чужую зону интерфейса.
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  authService.ensureSessionRestored();

  const expectedRole: UserRole = route.data['role'];
  const userRole = authService.userRole() ?? authService.getStoredUserRole();

  if (userRole === expectedRole) {
    return true;
  }

  // Перенаправляем пользователя на дашборд согласно фактической роли.
  switch (userRole) {
    case UserRole.Student:
      router.navigate(['/student/dashboard']);
      break;
    case UserRole.Teacher:
      router.navigate(['/teacher/dashboard']);
      break;
    case UserRole.Admin:
      router.navigate(['/admin/dashboard']);
      break;
    default:
      router.navigate(['/login']);
  }

  return false;
};
