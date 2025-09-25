import { Routes } from '@angular/router';

export const produtoresRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/produtor-list.component').then(m => m.ProdutorListComponent),
  },
  {
    path: 'novo',
    loadComponent: () => import('./components/produtor-detail.component').then(m => m.ProdutorDetailComponent),
    data: { mode: 'create' }
  },
  {
    path: 'editar/:id',
    loadComponent: () => import('./components/produtor-detail.component').then(m => m.ProdutorDetailComponent),
    data: { mode: 'edit' }
  }
];