import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'grants/:grantId/reports',
    loadComponent: () =>
      import('./features/reports/report-list.component').then(m => m.ReportListComponent)
  },
  {
    path: 'reports/:reportId',
    loadComponent: () =>
      import('./features/reports/report-form.component').then(m => m.ReportFormComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
