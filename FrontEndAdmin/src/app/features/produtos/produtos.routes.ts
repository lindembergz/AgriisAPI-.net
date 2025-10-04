import { Routes } from '@angular/router';
import { ProdutoListComponent } from './components/produto-list.component';
import { ProdutoDetailComponent } from './components/produto-detail.component';

export const produtosRoutes: Routes = [
  {
    path: '',
    component: ProdutoListComponent,
    title: 'Produtos - Agriis Admin'
  },
  {
    path: 'novo',
    component: ProdutoDetailComponent,
    title: 'Novo Produto - Agriis Admin'
  },
  {
    path: ':id',
    component: ProdutoDetailComponent,
    title: 'Editar Produto - Agriis Admin'
  }
];