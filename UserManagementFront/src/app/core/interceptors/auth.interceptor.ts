import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  if (req.url.includes('/auth/login') || (req.url.includes('/users') && req.method === 'POST')) {
    return next(req);
  }

  const token = tokenService.getToken();

  if (token) {
    // Clonar a requisição e adicionar o header Authorization
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    // Processa a requisição e tratar erros
    return next(cloned).pipe(
      catchError((error) => {
        if (error.status === 401) {
          tokenService.clearStorage();
          router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }

  return next(req);
};
