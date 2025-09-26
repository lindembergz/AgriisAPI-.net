import { Routes } from '@angular/router';

export const MUNICIPIOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./municipios.component').then(m => m.MunicipiosComponent),
    data: {
      title: 'Municípios',
      breadcrumb: 'Municípios'
    }
  }
];