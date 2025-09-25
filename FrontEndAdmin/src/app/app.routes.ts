import { Routes } from '@angular/router';
import { LayoutComponent } from './features/layout/layout.component';
import { LoginComponent } from './features/autenticacao/login.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard], // Only allow access if not authenticated
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard], // Protect all main routes
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'produtores',
        loadChildren: () => import('./features/produtores/produtores.routes').then(m => m.produtoresRoutes),
      },
      {
        path: 'fornecedores',
        loadChildren: () => import('./features/fornecedores/fornecedores.routes').then(m => m.fornecedoresRoutes),
      },
      {
        path: 'test-map',
        loadComponent: () => import('./shared/components/test-map.component').then(m => m.TestMapComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '', // Redirect unknown routes to home
  },
];
