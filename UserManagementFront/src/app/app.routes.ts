import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Rotas publicas
  { path: '', component: HomeComponent },
  
  // Rotas protegidas
  { 
    path: 'user-info', 
    loadComponent: () => import('./features/users/pages/user-info/user-info.component')
      .then(m => m.UserInfoComponent),
    canActivate: [authGuard]
  },
  
  // Redirecionamento para home em caso de rota não encontrada
  { path: '**', redirectTo: '' }
];