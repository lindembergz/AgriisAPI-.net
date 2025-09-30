import { Routes } from '@angular/router';

export const PAISES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./paises.component').then(m => m.PaisesComponent),
    data: {
      title: 'Países',
      breadcrumb: 'Países'
    }
  }
];