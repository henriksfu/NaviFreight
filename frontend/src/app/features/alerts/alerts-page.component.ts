import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AlertService } from '../../core/services/alert.service';
import { AlertResponse, AlertStatus, AlertSeverity } from '../../core/models/alert.models';
import { PaginationComponent } from '../../shared/components/pagination.component';

type StatusFilter = 'All' | AlertStatus;
type SeverityFilter = 'All' | AlertSeverity;

@Component({
  selector: 'app-alerts-page',
  standalone: true,
  imports: [NgClass, FormsModule, PaginationComponent],
  templateUrl: './alerts-page.component.html',
  styleUrl: './alerts-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AlertsPageComponent implements OnInit {
  private readonly alertService = inject(AlertService);
  private readonly authService = inject(AuthService);

  protected readonly isAdmin = computed(() => this.authService.isTenantAdmin());
  protected readonly isDispatch = computed(() => {
    const role = this.authService.userRole();
    return role === 'Tenant Admin' || role === 'Dispatcher';
  });
  private get userEmail(): string {
    return this.authService.currentUser()?.email ?? '';
  }

  protected readonly alerts = signal<AlertResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected currentPage  = 1;
  protected totalPages   = 1;
  protected readonly pageSize = 20;

  // Filters
  protected readonly statusFilter = signal<StatusFilter>('All');
  protected readonly severityFilter = signal<SeverityFilter>('All');

  protected readonly statusTabs: StatusFilter[] = ['All', 'Active', 'Acknowledged', 'Resolved', 'Closed'];
  protected readonly severityOptions: SeverityFilter[] = ['All', 'Critical', 'Warning', 'Info'];

  protected readonly filteredAlerts = computed(() => {
    let list = this.alerts();
    const sv = this.severityFilter();
    if (sv !== 'All') list = list.filter(a => a.severity === sv);
    return list;
  });

  protected readonly activeCount = computed(() =>
    this.alerts().filter(a => a.status === 'Active').length
  );

  // Create form
  protected readonly showCreateForm = signal(false);
  protected createSeverity = 'Warning';
  protected createTitle = '';
  protected createDescription = '';
  protected createOwner = '';
  protected readonly isSaving = signal(false);
  protected readonly formError = signal<string | null>(null);

  // Per-alert action panels
  protected readonly resolvingAlertId = signal<number | null>(null);
  protected resolveNotes = '';

  protected readonly reassigningAlertId = signal<number | null>(null);
  protected reassignOwner = '';

  // Busy tracking
  protected readonly busyAlertId = signal<number | null>(null);

  readonly severities = ['Critical', 'Warning', 'Info'];

  ngOnInit(): void {
    this.load();
  }

  protected trackById(_: number, alert: AlertResponse): number {
    return alert.alertEventId;
  }

  protected severityClass(severity: string): string {
    return 'sev-' + severity.toLowerCase();
  }

  protected statusClass(status: string): string {
    return 'st-' + status.toLowerCase();
  }

  protected formatDate(utc: string | null): string {
    if (!utc) return '—';
    return new Date(utc).toLocaleString(undefined, {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  // ── Filters ───────────────────────────────────────────────────────────────

  protected setStatus(s: StatusFilter): void {
    this.statusFilter.set(s);
    this.currentPage = 1;
    this.load(s === 'All' ? undefined : s);
  }

  protected setSeverity(s: SeverityFilter): void {
    this.severityFilter.set(s);
  }

  protected async onPage(page: number): Promise<void> {
    this.currentPage = page;
    await this.load(this.statusFilter() === 'All' ? undefined : this.statusFilter());
  }

  // ── Load ──────────────────────────────────────────────────────────────────

  private async load(status?: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const result = await this.alertService.getAlerts(status, undefined, this.currentPage, this.pageSize);
      this.alerts.set(result.items);
      this.totalPages = result.totalPages;
    } catch {
      this.error.set('Failed to load alerts. Check that the backend is running.');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ── Create ────────────────────────────────────────────────────────────────

  protected openCreate(): void {
    this.createSeverity = 'Warning';
    this.createTitle = '';
    this.createDescription = '';
    this.createOwner = '';
    this.formError.set(null);
    this.showCreateForm.set(true);
  }

  protected cancelCreate(): void {
    this.showCreateForm.set(false);
    this.formError.set(null);
  }

  protected async submitCreate(): Promise<void> {
    if (!this.createTitle.trim()) { this.formError.set('Title is required.'); return; }
    if (!this.createDescription.trim()) { this.formError.set('Description is required.'); return; }
    if (!this.createOwner.trim()) { this.formError.set('Owner team is required.'); return; }

    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const created = await this.alertService.createAlert({
        severity: this.createSeverity,
        title: this.createTitle.trim(),
        description: this.createDescription.trim(),
        ownerTeam: this.createOwner.trim()
      });
      this.alerts.update(list => [created, ...list]);
      this.showCreateForm.set(false);
    } catch {
      this.formError.set('Failed to create alert.');
    } finally {
      this.isSaving.set(false);
    }
  }

  // ── Acknowledge (one click — uses current user email) ─────────────────────

  protected async acknowledge(alertId: number): Promise<void> {
    this.busyAlertId.set(alertId);
    try {
      const updated = await this.alertService.acknowledge(alertId, {
        acknowledgedByEmail: this.userEmail
      });
      this.replaceAlert(updated);
    } catch {
      this.error.set('Failed to acknowledge alert.');
    } finally {
      this.busyAlertId.set(null);
    }
  }

  // ── Resolve (inline form for notes) ──────────────────────────────────────

  protected openResolve(alertId: number): void {
    this.resolveNotes = '';
    this.resolvingAlertId.set(alertId);
  }

  protected cancelResolve(): void {
    this.resolvingAlertId.set(null);
  }

  protected async submitResolve(alertId: number): Promise<void> {
    this.busyAlertId.set(alertId);
    try {
      const updated = await this.alertService.resolve(alertId, {
        resolvedByEmail: this.userEmail,
        resolutionNotes: this.resolveNotes.trim() || null
      });
      this.replaceAlert(updated);
      this.resolvingAlertId.set(null);
    } catch {
      this.error.set('Failed to resolve alert.');
    } finally {
      this.busyAlertId.set(null);
    }
  }

  // ── Reopen ────────────────────────────────────────────────────────────────

  protected async reopen(alertId: number): Promise<void> {
    this.busyAlertId.set(alertId);
    try {
      const updated = await this.alertService.reopen(alertId);
      this.replaceAlert(updated);
    } catch {
      this.error.set('Failed to reopen alert.');
    } finally {
      this.busyAlertId.set(null);
    }
  }

  // ── Reassign owner ────────────────────────────────────────────────────────

  protected openReassign(alert: AlertResponse): void {
    this.reassignOwner = alert.ownerTeam;
    this.reassigningAlertId.set(alert.alertEventId);
  }

  protected cancelReassign(): void {
    this.reassigningAlertId.set(null);
  }

  protected async submitReassign(alertId: number): Promise<void> {
    if (!this.reassignOwner.trim()) return;
    this.busyAlertId.set(alertId);
    try {
      const updated = await this.alertService.updateOwner(alertId, {
        ownerTeam: this.reassignOwner.trim()
      });
      this.replaceAlert(updated);
      this.reassigningAlertId.set(null);
    } catch {
      this.error.set('Failed to reassign owner.');
    } finally {
      this.busyAlertId.set(null);
    }
  }

  // ── Close (admin only) ────────────────────────────────────────────────────

  protected async close(alertId: number): Promise<void> {
    this.busyAlertId.set(alertId);
    try {
      await this.alertService.close(alertId);
      this.alerts.update(list => list.filter(a => a.alertEventId !== alertId));
    } catch {
      this.error.set('Failed to close alert.');
    } finally {
      this.busyAlertId.set(null);
    }
  }

  private replaceAlert(updated: AlertResponse): void {
    this.alerts.update(list =>
      list.map(a => a.alertEventId === updated.alertEventId ? updated : a)
    );
  }
}
