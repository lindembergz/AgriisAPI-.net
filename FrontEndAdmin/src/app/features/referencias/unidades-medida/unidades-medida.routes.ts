import { Routes } from '@angular/router';

export const UNIDADES_MEDIDA_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./unidades-medida.component').then(m => m.UnidadesMedidaComponent),
    data: {
      title: 'Unidades de Medida',
      breadcrumb: 'Unidades de Medida'
    }
  }
];