import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DashboardService } from '../core/services/dashboard.service';
import { AuthService } from '../core/services/auth.service';
import { ToastContainerComponent } from '../shared/components/toast-container.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, ToastContainerComponent],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppShellComponent {
  private readonly dashboardService = inject(DashboardService);
  protected readonly authService = inject(AuthService);

  protected readonly dashboard = this.dashboardService.dashboard;
  protected readonly isMobileOpen = signal(false);

  protected readonly groupedNavigation = computed(() => {
    const navigation = this.dashboardService.navigation();
    const isAdmin = this.authService.isTenantAdmin();

    const groups = [
      { label: 'Operations', items: navigation.filter((i) => i.section === 'Operations') },
      { label: 'Analytics',  items: navigation.filter((i) => i.section === 'Analytics') }
    ];

    if (isAdmin) {
      groups.push({ label: 'Admin', items: navigation.filter((i) => i.section === 'Admin') });
    }

    return groups;
  });

  protected toggleMobile(): void {
    this.isMobileOpen.update((v) => !v);
  }

  protected closeMobile(): void {
    this.isMobileOpen.set(false);
  }

  protected logout(): void {
    this.authService.logout();
  }
}
