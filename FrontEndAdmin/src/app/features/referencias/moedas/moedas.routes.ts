import { Routes } from '@angular/router';

export const MOEDAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./moedas.component').then(m => m.MoedasComponent),
    data: {
      title: 'Moedas',
      breadcrumb: 'Moedas'
    }
  }
];