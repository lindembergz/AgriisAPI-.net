import { Routes } from '@angular/router';

export const CULTURAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/culturas-list/culturas-list.component').then(m => m.CulturasListComponent),
    title: 'Culturas - Agriis'
  },
  {
    path: 'nova',
    loadComponent: () => import('./components/culturas-detail/culturas-detail.component').then(m => m.CulturasDetailComponent),
    title: 'Nova Cultura - Agriis'
  },
  {
    path: ':id',
    loadComponent: () => import('./components/culturas-detail/culturas-detail.component').then(m => m.CulturasDetailComponent),
    title: 'Editar Cultura - Agriis'
  }
];