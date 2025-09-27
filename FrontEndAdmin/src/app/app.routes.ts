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
        path: 'culturas',
        loadChildren: () => import('./features/culturas/culturas.routes').then(m => m.CULTURAS_ROUTES),
      },
      {
        path: 'safras',
        loadChildren: () => import('./features/safras/safras.routes').then(m => m.safrasRoutes),
      },
      {
        path: 'produtos',
        loadChildren: () => import('./features/produtos/produtos.routes').then(m => m.produtosRoutes),
      },
      {
        path: 'referencias',
        children: [
          {
            path: 'unidades-medida',
            loadChildren: () => import('./features/referencias/unidades-medida/unidades-medida.routes').then(m => m.UNIDADES_MEDIDA_ROUTES),
          },
          {
            path: 'moedas',
            loadChildren: () => import('./features/referencias/moedas/moedas.routes').then(m => m.MOEDAS_ROUTES),
          },
          {
            path: 'paises',
            loadChildren: () => import('./features/referencias/paises/paises.routes').then(m => m.PAISES_ROUTES),
          },
          {
            path: 'ufs',
            loadChildren: () => import('./features/referencias/ufs/ufs.routes').then(m => m.UFS_ROUTES),
          },
          {
            path: 'municipios',
            loadChildren: () => import('./features/referencias/municipios/municipios.routes').then(m => m.MUNICIPIOS_ROUTES),
          },
          {
            path: 'atividades-agropecuarias',
            loadChildren: () => import('./features/referencias/atividades-agropecuarias/atividades-agropecuarias.routes').then(m => m.ATIVIDADES_AGROPECUARIAS_ROUTES),
          },
          {
            path: 'embalagens',
            loadChildren: () => import('./features/referencias/embalagens/embalagens.routes').then(m => m.EMBALAGENS_ROUTES),
          },
          {
            path: 'categorias',
            loadChildren: () => import('./features/referencias/categorias/categorias.routes').then(m => m.CATEGORIAS_ROUTES),
          },
          {
            path: '',
            redirectTo: 'unidades-medida',
            pathMatch: 'full'
          }
        ]
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
