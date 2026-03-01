import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  // Rotas públicas
  { path: '', component: HomeComponent },

  // Rotas protegidas - acessíveis apenas para usuários autenticados
  {
    path: 'user-info',
    loadComponent: () => import('./features/users/pages/user-info/user-info.component')
      .then(m => m.UserInfoComponent),
    canActivate: [authGuard]
  },

  // Rotas de admin
  {
    path: 'dashboard',
    loadComponent: () => import('./features/users/pages/dashboard/dashboard.component')
      .then(m => m.DashboardComponent),
    canActivate: [authGuard, adminGuard]
  },

  { path: '**', redirectTo: '' }
];
