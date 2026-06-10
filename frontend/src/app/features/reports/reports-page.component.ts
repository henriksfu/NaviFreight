import {
  ChangeDetectionStrategy, Component, inject, OnInit, signal, computed
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface ReportSummary {
  from: string; to: string;
  totalAlerts: number; criticalAlerts: number; warningAlerts: number; infoAlerts: number;
  totalVehicles: number; inTransitVehicles: number; atDockVehicles: number;
  awaitingDispatchVehicles: number; delayedVehicles: number;
  totalRoutes: number; activeRoutes: number; onScheduleRoutes: number;
  totalYards: number; avgYardOccupancyPercent: number;
}

interface ReportSnapshot { reportName: string; value: string; changeLabel: string; }

type QuickRange = '7d' | '30d' | '90d' | 'custom';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportsPageComponent implements OnInit {
  private readonly http = inject(HttpClient);

  protected readonly isLoading   = signal(true);
  protected readonly error       = signal<string | null>(null);
  protected readonly summary     = signal<ReportSummary | null>(null);
  protected readonly snapshots   = signal<ReportSnapshot[]>([]);
  protected readonly activeRange = signal<QuickRange>('30d');

  protected fromDate = this.toInputDate(this.daysAgo(30));
  protected toDate   = this.toInputDate(new Date());

  protected readonly onTimePercent = computed(() => {
    const s = this.summary();
    if (!s || s.activeRoutes === 0) return 0;
    return Math.round((s.onScheduleRoutes / s.totalRoutes) * 100);
  });

  protected readonly alertBarWidths = computed(() => {
    const s = this.summary();
    if (!s || s.totalAlerts === 0) return { critical: 0, warning: 0, info: 0 };
    return {
      critical: Math.round((s.criticalAlerts / s.totalAlerts) * 100),
      warning:  Math.round((s.warningAlerts  / s.totalAlerts) * 100),
      info:     Math.round((s.infoAlerts     / s.totalAlerts) * 100),
    };
  });

  protected readonly fleetBarWidths = computed(() => {
    const s = this.summary();
    if (!s || s.totalVehicles === 0) return { transit: 0, dock: 0, waiting: 0, delayed: 0 };
    const t = s.totalVehicles;
    return {
      transit:  Math.round((s.inTransitVehicles         / t) * 100),
      dock:     Math.round((s.atDockVehicles            / t) * 100),
      waiting:  Math.round((s.awaitingDispatchVehicles  / t) * 100),
      delayed:  Math.round((s.delayedVehicles           / t) * 100),
    };
  });

  async ngOnInit(): Promise<void> {
    await Promise.all([this.loadSummary(), this.loadSnapshots()]);
  }

  protected async selectRange(range: QuickRange): Promise<void> {
    this.activeRange.set(range);
    if (range !== 'custom') {
      const days = range === '7d' ? 7 : range === '30d' ? 30 : 90;
      this.fromDate = this.toInputDate(this.daysAgo(days));
      this.toDate   = this.toInputDate(new Date());
      await this.loadSummary();
    }
  }

  protected async applyCustomRange(): Promise<void> {
    await this.loadSummary();
  }

  private async loadSummary(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const params = new HttpParams()
        .set('from', new Date(this.fromDate).toISOString())
        .set('to',   new Date(this.toDate).toISOString());
      const data = await firstValueFrom(
        this.http.get<ReportSummary>('/api/reports/summary', { params })
      );
      this.summary.set(data);
    } catch {
      this.error.set('Failed to load report data.');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadSnapshots(): Promise<void> {
    try {
      const data = await firstValueFrom(
        this.http.get<{ items: ReportSnapshot[] }>('/api/reports/snapshots')
      );
      this.snapshots.set(data.items);
    } catch { /* non-critical */ }
  }

  private daysAgo(n: number): Date {
    const d = new Date();
    d.setDate(d.getDate() - n);
    return d;
  }

  private toInputDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
