import { Routes } from '@angular/router';

export const fornecedoresRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/fornecedor-list.component').then(m => m.FornecedorListComponent),
  },
  {
    path: 'novo',
    loadComponent: () => import('./components/fornecedor-detail.component').then(m => m.FornecedorDetailComponent),
    data: { mode: 'create' }
  },
  {
    path: 'editar/:id',
    loadComponent: () => import('./components/fornecedor-detail.component').then(m => m.FornecedorDetailComponent),
    data: { mode: 'edit' }
  }
];