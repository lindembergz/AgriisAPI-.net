import { Routes } from '@angular/router';

export const safrasRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/safras-list/safras-list.component').then(m => m.SafrasListComponent),
    title: 'Safras - Agriis'
  },
  {
    path: 'nova',
    loadComponent: () => import('./components/safras-detail/safras-detail.component').then(m => m.SafrasDetailComponent),
    title: 'Nova Safra - Agriis'
  },
  {
    path: ':id',
    loadComponent: () => import('./components/safras-detail/safras-detail.component').then(m => m.SafrasDetailComponent),
    title: 'Editar Safra - Agriis'
  }
];