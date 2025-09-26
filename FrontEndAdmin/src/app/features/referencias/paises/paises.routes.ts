import { Routes } from '@angular/router';
import { PaisesComponent } from './paises.component';

export const PAISES_ROUTES: Routes = [
  {
    path: '',
    component: PaisesComponent,
    data: {
      title: 'Países',
      breadcrumb: 'Países'
    }
  }
];