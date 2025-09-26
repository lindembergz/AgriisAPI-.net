import { Routes } from '@angular/router';

export const ATIVIDADES_AGROPECUARIAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./atividades-agropecuarias.component').then(m => m.AtividadesAgropecuariasComponent),
    data: {
      title: 'Atividades Agropecuárias',
      breadcrumb: 'Atividades Agropecuárias'
    }
  }
];