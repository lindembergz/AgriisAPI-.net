import { Routes } from '@angular/router';
import { UfsComponent } from './ufs.component';

export const UFS_ROUTES: Routes = [
  {
    path: '',
    component: UfsComponent,
    data: {
      title: 'UFs',
      breadcrumb: 'UFs'
    }
  }
];