import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);
  
  const user = tokenService.getUser();
  
  if (user?.role === 'Admin') {
    return true;
  }
  
  // Redirecionar para user-info se não for admin
  router.navigate(['/user-info']);
  return false;
};