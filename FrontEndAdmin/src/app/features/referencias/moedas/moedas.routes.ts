import { Routes } from '@angular/router';
import { MoedasComponent } from './moedas.component';

export const MOEDAS_ROUTES: Routes = [
  {
    path: '',
    component: MoedasComponent,
    data: {
      title: 'Moedas',
      breadcrumb: 'Moedas'
    }
  }
];