import { Routes } from '@angular/router';
import { AppShellComponent } from './layout/app-shell.component';
import { AlertsPageComponent } from './features/alerts/alerts-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { FleetPageComponent } from './features/fleet/fleet-page.component';
import { LoginPageComponent } from './features/login/login-page.component';
import { ReportsPageComponent } from './features/reports/reports-page.component';
import { RoutesPageComponent } from './features/routes/routes-page.component';
import { SettingsPageComponent } from './features/settings/settings-page.component';
import { YardsPageComponent } from './features/yards/yards-page.component';
import { YardDetailPageComponent } from './features/yards/yard-detail-page.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginPageComponent,
    title: 'NaviFreight | Sign In'
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        component: DashboardPageComponent,
        title: 'NaviFreight | Fleet & Yard Command Center'
      },
      {
        path: 'fleet',
        component: FleetPageComponent,
        title: 'NaviFreight | Fleet Board'
      },
      {
        path: 'yards',
        component: YardsPageComponent,
        title: 'NaviFreight | Yard Management'
      },
      {
        path: 'yards/:yardId',
        component: YardDetailPageComponent,
        title: 'NaviFreight | Yard Detail'
      },
      {
        path: 'routes',
        component: RoutesPageComponent,
        title: 'NaviFreight | Routing'
      },
      {
        path: 'reports',
        component: ReportsPageComponent,
        title: 'NaviFreight | Reports'
      },
      {
        path: 'alerts',
        component: AlertsPageComponent,
        title: 'NaviFreight | Alerts'
      },
      {
        path: 'settings',
        component: SettingsPageComponent,
        canActivate: [roleGuard(['Tenant Admin'])],
        title: 'NaviFreight | Settings'
      }
    ]
  }
];
