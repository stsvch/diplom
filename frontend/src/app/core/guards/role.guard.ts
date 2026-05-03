import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user.model';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  authService.ensureSessionRestored();

  const expectedRole: UserRole = route.data['role'];
  const userRole = authService.userRole() ?? authService.getStoredUserRole();

  if (userRole === expectedRole) {
    return true;
  }

  // Redirect to appropriate dashboard based on actual role
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
