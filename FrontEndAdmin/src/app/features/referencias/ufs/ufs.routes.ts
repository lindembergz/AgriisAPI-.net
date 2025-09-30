import { Routes } from '@angular/router';

export const UFS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./ufs.component').then(m => m.UfsComponent),
    data: {
      title: 'UFs',
      breadcrumb: 'UFs'
    }
  }
];