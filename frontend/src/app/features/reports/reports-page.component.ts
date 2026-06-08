import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportsPageComponent {
  private readonly dashboardService = inject(DashboardService);

  protected readonly dashboard = this.dashboardService.dashboard;
}
