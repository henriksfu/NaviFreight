import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { KpiCardComponent } from '../../shared/components/kpi-card.component';
import { YardService } from '../../core/services/yard.service';
import { FleetService } from '../../core/services/fleet.service';
import { AlertService } from '../../core/services/alert.service';
import { RouteService } from '../../core/services/route.service';
import { YardResponse } from '../../core/models/yard.models';
import { FleetVehicleResponse } from '../../core/models/fleet.models';
import { AlertResponse } from '../../core/models/alert.models';
import { RouteAssignmentResponse } from '../../core/models/route.models';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [NgClass, KpiCardComponent],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPageComponent implements OnInit {
  private readonly yardService  = inject(YardService);
  private readonly fleetService = inject(FleetService);
  private readonly alertService = inject(AlertService);
  private readonly routeService = inject(RouteService);

  protected readonly yards    = signal<YardResponse[]>([]);
  protected readonly vehicles = signal<FleetVehicleResponse[]>([]);
  protected readonly alerts   = signal<AlertResponse[]>([]);
  protected readonly routes   = signal<RouteAssignmentResponse[]>([]);

  protected readonly isLoading = signal(true);
  protected readonly error     = signal<string | null>(null);

  protected readonly summaryCards = computed(() => {
    const v = this.vehicles();
    const y = this.yards();
    const a = this.alerts();
    const r = this.routes();

    const activeVehicles = v.filter(x => x.status !== 'Available').length;

    const avgOccupancy = y.length === 0 ? 0
      : Math.round(y.reduce((sum, x) => sum + (x.capacity > 0 ? (x.occupiedSlots / x.capacity) * 100 : 0), 0) / y.length);

    const criticalAlerts = a.filter(x => x.severity === 'Critical').length;

    const onTimeRoutes = r.length === 0 ? 100
      : Math.round((r.filter(x => x.status === 'On Schedule').length / r.length) * 100);

    const avgTurn = y.length === 0 ? 0
      : Math.round(y.reduce((sum, x) => sum + x.averageTurnMinutes, 0) / y.length);

    return [
      {
        label: 'Active Vehicles',
        value: String(activeVehicles),
        detail: `${v.length} total tracked assets`,
        variant: 'accent' as const,
        trend: `${v.filter(x => x.status === 'In Transit').length} in transit`
      },
      {
        label: 'Yard Occupancy',
        value: `${avgOccupancy}%`,
        detail: 'avg slot utilization',
        variant: (avgOccupancy >= 85 ? 'alert' : 'default') as 'alert' | 'default',
        trend: `across ${y.length} yards`
      },
      {
        label: 'Critical Alerts',
        value: String(criticalAlerts),
        detail: `${a.length} total active alerts`,
        variant: (criticalAlerts > 0 ? 'alert' : 'default') as 'alert' | 'default',
        trend: criticalAlerts > 0 ? 'Requires immediate attention' : 'All clear'
      },
      {
        label: 'On-Time Dispatch',
        value: `${onTimeRoutes}%`,
        detail: 'routes on schedule',
        variant: (onTimeRoutes < 70 ? 'alert' : 'accent') as 'alert' | 'accent',
        trend: `${r.filter(x => x.status === 'Delayed').length} delayed`
      },
      {
        label: 'Active Routes',
        value: String(r.length),
        detail: 'currently releasing freight',
        variant: 'default' as const,
        trend: `${r.filter(x => x.status === 'At Risk').length} at risk`
      },
      {
        label: 'Avg Trailer Turn',
        value: `${avgTurn}m`,
        detail: 'dock cycle average',
        variant: 'default' as const,
        trend: `${y.reduce((s, x) => s + x.inboundQueue, 0)} total inbound`
      }
    ];
  });

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const [yardsResult, vehiclesResult, alertsResult, routesResult] = await Promise.all([
        this.yardService.getYards(),
        this.fleetService.getVehicles(),
        this.alertService.getAlerts('Active'),
        this.routeService.getRoutes()
      ]);
      this.yards.set(yardsResult.items);
      this.vehicles.set(vehiclesResult.items);
      this.alerts.set(alertsResult.items);
      this.routes.set(routesResult.items);
    } catch {
      this.error.set('Failed to load dashboard data. Check that the backend is running.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected statusClass(status: string): string {
    return 's-' + status.toLowerCase().replace(/\s+/g, '-');
  }

  protected severityClass(severity: string): string {
    return 'sev-' + severity.toLowerCase();
  }

  protected yardFillClass(occupied: number, total: number): string {
    const pct = total > 0 ? occupied / total : 0;
    if (pct >= 0.85) return 'fill-hi';
    if (pct >= 0.6)  return 'fill-mid';
    return 'fill-ok';
  }

  protected formatEta(utc: string | null): string {
    if (!utc) return '—';
    const d = new Date(utc);
    return d.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
  }
}
