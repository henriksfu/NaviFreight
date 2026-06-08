import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.models';

export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn =>
  () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const role = authService.userRole();

    if (role && allowedRoles.includes(role)) return true;

    router.navigate(['/']);
    return false;
  };
