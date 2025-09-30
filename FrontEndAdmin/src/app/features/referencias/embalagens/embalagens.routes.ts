import { Routes } from '@angular/router';

export const EMBALAGENS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./embalagens.component').then(m => m.EmbalagensComponent),
    data: {
      title: 'Embalagens',
      breadcrumb: 'Embalagens'
    }
  }
];