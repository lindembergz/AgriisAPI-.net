import { Routes } from '@angular/router';
import { EmbalagensComponent } from './embalagens.component';

export const EMBALAGENS_ROUTES: Routes = [
  {
    path: '',
    component: EmbalagensComponent,
    data: {
      title: 'Embalagens',
      breadcrumb: 'Embalagens'
    }
  }
];