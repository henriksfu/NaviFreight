import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPageComponent {
  private readonly dashboardService = inject(DashboardService);

  protected readonly dashboard = this.dashboardService.dashboard;
}
