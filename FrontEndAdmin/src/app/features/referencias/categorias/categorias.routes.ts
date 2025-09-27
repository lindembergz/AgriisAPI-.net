import { Routes } from '@angular/router';
import { CategoriasComponent } from './categorias.component';

export const CATEGORIAS_ROUTES: Routes = [
  {
    path: '',
    component: CategoriasComponent,
    data: {
      title: 'Categorias',
      breadcrumb: 'Categorias',
      permissions: ['ADMIN', 'GERENTE_PRODUTOS']
    }
  }
];