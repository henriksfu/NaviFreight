import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast   = inject(ToastService);
  const auth    = inject(AuthService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const isLoginRequest = req.url.includes('/auth/login');

      if (err.status === 401 && !isLoginRequest) {
        auth.logout();
        toast.error('Your session has expired. Please sign in again.');
      } else if (err.status === 403) {
        toast.error('You don\'t have permission to perform this action.');
      } else if (err.status === 0) {
        toast.error('Network error — check your connection and try again.');
      } else if (err.status >= 500) {
        toast.error('Something went wrong on the server. Please try again.');
      }

      return throwError(() => err);
    })
  );
};
