import { Injectable, signal } from '@angular/core';
import { DashboardViewModel } from '../models/dashboard.models';
import { dashboardData } from '../data/dashboard.data';
import { navigationItems } from '../data/navigation.data';
import { NavigationItem } from '../models/navigation.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly dashboardState = signal<DashboardViewModel>(dashboardData);
  private readonly navigationState = signal<NavigationItem[]>(navigationItems);

  readonly dashboard = this.dashboardState.asReadonly();
  readonly navigation = this.navigationState.asReadonly();
}
