import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/produtos/listagem/listagem').then(m => m.ListagemComponent)
  },
  {
    path: 'produtos/novo',
    loadComponent: () =>
      import('./features/produtos/formulario/formulario').then(m => m.FormularioComponent)
  }
];