import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { RouteService } from '../../core/services/route.service';
import { RouteAssignmentResponse, RouteDispatchAssignmentResponse } from '../../core/models/route.models';
import { PaginationComponent } from '../../shared/components/pagination.component';

interface RouteWithDetail {
  route: RouteAssignmentResponse;
  expanded: boolean;
  assignedVehicles: RouteDispatchAssignmentResponse[] | null;
  loadingDetail: boolean;
}

@Component({
  selector: 'app-routes-page',
  standalone: true,
  imports: [NgClass, FormsModule, PaginationComponent],
  templateUrl: './routes-page.component.html',
  styleUrl: './routes-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoutesPageComponent implements OnInit {
  private readonly routeService = inject(RouteService);
  private readonly authService = inject(AuthService);

  protected readonly isAdmin = computed(() => this.authService.isTenantAdmin());
  protected readonly isDispatch = computed(() => {
    const role = this.authService.userRole();
    return role === 'Tenant Admin' || role === 'Dispatcher';
  });

  protected readonly routes = signal<RouteWithDetail[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected currentPage = 1;
  protected totalPages  = 1;
  protected readonly pageSize = 20;

  // Create form
  protected readonly showCreateForm = signal(false);
  protected newRouteCode = '';
  protected newOriginYardId = '';
  protected newDestination = '';
  protected newStatus = 'On Schedule';
  protected newDeparture = '';
  protected newCompletion = 0;
  protected readonly isSaving = signal(false);
  protected readonly formError = signal<string | null>(null);

  // Edit route inline
  protected readonly editingRouteCode = signal<string | null>(null);
  protected editDestination = '';
  protected editStatus = '';
  protected editDeparture = '';
  protected editCompletion = 0;
  protected readonly isSavingEdit = signal(false);

  // Assign vehicles inline
  protected readonly assigningRouteCode = signal<string | null>(null);
  protected assignVehicleIds = '';
  protected readonly isSavingAssign = signal(false);
  protected readonly assignError = signal<string | null>(null);

  protected readonly deletingRouteCode = signal<string | null>(null);
  protected readonly unassigningKey = signal<string | null>(null);

  readonly routeStatuses = ['On Schedule', 'At Risk', 'Delayed', 'Completed'];

  ngOnInit(): void {
    this.load();
  }

  protected trackByCode(_: number, r: RouteWithDetail): string {
    return r.route.routeCode;
  }

  protected statusClass(status: string): string {
    return 'rs-' + status.toLowerCase().replace(/\s+/g, '-');
  }

  protected formatDate(utc: string): string {
    return new Date(utc).toLocaleString(undefined, {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  protected toDatetimeLocal(utc: string): string {
    if (!utc) return '';
    const d = new Date(utc);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  protected async onPage(page: number): Promise<void> {
    this.currentPage = page;
    await this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const result = await this.routeService.getRoutes(this.currentPage, this.pageSize);
      this.routes.set(result.items.map(r => ({
        route: r, expanded: false, assignedVehicles: null, loadingDetail: false
      })));
      this.totalPages = result.totalPages;
    } catch {
      this.error.set('Failed to load routes. Check that the backend is running.');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ── Expand to show assigned vehicles ──────────────────────────────────────

  protected async toggleExpand(routeCode: string): Promise<void> {
    const item = this.routes().find(r => r.route.routeCode === routeCode);
    if (!item) return;

    if (item.expanded) {
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode ? { ...r, expanded: false } : r
      ));
      return;
    }

    if (item.assignedVehicles !== null) {
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode ? { ...r, expanded: true } : r
      ));
      return;
    }

    this.routes.update(list => list.map(r =>
      r.route.routeCode === routeCode ? { ...r, loadingDetail: true } : r
    ));
    try {
      const detail = await this.routeService.getRoute(routeCode);
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode
          ? { ...r, expanded: true, assignedVehicles: [...detail.assignedVehicles], loadingDetail: false }
          : r
      ));
    } catch {
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode ? { ...r, loadingDetail: false } : r
      ));
      this.error.set('Failed to load route detail.');
    }
  }

  // ── Create route ──────────────────────────────────────────────────────────

  protected openCreate(): void {
    this.newRouteCode = '';
    this.newOriginYardId = '';
    this.newDestination = '';
    this.newStatus = 'On Schedule';
    this.newDeparture = '';
    this.newCompletion = 0;
    this.formError.set(null);
    this.showCreateForm.set(true);
  }

  protected cancelCreate(): void {
    this.showCreateForm.set(false);
    this.formError.set(null);
  }

  protected async submitCreate(): Promise<void> {
    if (!this.newRouteCode.trim()) { this.formError.set('Route code is required.'); return; }
    const originId = parseInt(this.newOriginYardId, 10);
    if (isNaN(originId) || originId <= 0) { this.formError.set('A valid origin yard ID is required.'); return; }
    if (!this.newDestination.trim()) { this.formError.set('Destination is required.'); return; }
    if (!this.newDeparture) { this.formError.set('Departure date/time is required.'); return; }

    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const created = await this.routeService.createRoute({
        routeCode: this.newRouteCode.trim(),
        originYardId: originId,
        destinationName: this.newDestination.trim(),
        status: this.newStatus,
        nextDepartureUtc: new Date(this.newDeparture).toISOString(),
        completionPercent: this.newCompletion
      });
      this.routes.update(list => [{
        route: created, expanded: false, assignedVehicles: null, loadingDetail: false
      }, ...list]);
      this.showCreateForm.set(false);
    } catch {
      this.formError.set('Failed to create route. The route code may already exist.');
    } finally {
      this.isSaving.set(false);
    }
  }

  // ── Edit route ────────────────────────────────────────────────────────────

  protected openEdit(r: RouteAssignmentResponse): void {
    this.editDestination = r.destination;
    this.editStatus = r.status;
    this.editDeparture = this.toDatetimeLocal(r.nextDepartureUtc);
    this.editCompletion = r.completionPercent;
    this.editingRouteCode.set(r.routeCode);
  }

  protected cancelEdit(): void {
    this.editingRouteCode.set(null);
  }

  protected async submitEdit(routeCode: string): Promise<void> {
    if (!this.editDestination.trim()) return;
    this.isSavingEdit.set(true);
    try {
      const updated = await this.routeService.updateRoute(routeCode, {
        destinationName: this.editDestination.trim(),
        status: this.editStatus,
        nextDepartureUtc: new Date(this.editDeparture).toISOString(),
        completionPercent: this.editCompletion
      });
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode ? { ...r, route: updated } : r
      ));
      this.editingRouteCode.set(null);
    } catch {
      this.error.set('Failed to update route.');
    } finally {
      this.isSavingEdit.set(false);
    }
  }

  // ── Delete route ──────────────────────────────────────────────────────────

  protected async deleteRoute(routeCode: string): Promise<void> {
    this.deletingRouteCode.set(routeCode);
    try {
      await this.routeService.deleteRoute(routeCode);
      this.routes.update(list => list.filter(r => r.route.routeCode !== routeCode));
    } catch {
      this.error.set('Failed to delete route.');
    } finally {
      this.deletingRouteCode.set(null);
    }
  }

  // ── Assign vehicles ───────────────────────────────────────────────────────

  protected openAssign(routeCode: string): void {
    this.assignVehicleIds = '';
    this.assignError.set(null);
    this.assigningRouteCode.set(routeCode);
  }

  protected cancelAssign(): void {
    this.assigningRouteCode.set(null);
    this.assignError.set(null);
  }

  protected async submitAssign(routeCode: string): Promise<void> {
    const ids = this.assignVehicleIds.split(',').map(s => s.trim()).filter(Boolean);
    if (ids.length === 0) { this.assignError.set('Enter at least one vehicle ID.'); return; }

    this.isSavingAssign.set(true);
    this.assignError.set(null);
    try {
      const detail = await this.routeService.assignVehicles(routeCode, ids);
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode
          ? { ...r, route: detail.route, assignedVehicles: [...detail.assignedVehicles], expanded: true }
          : r
      ));
      this.assigningRouteCode.set(null);
    } catch {
      this.assignError.set('Failed to assign vehicles. Check the vehicle IDs.');
    } finally {
      this.isSavingAssign.set(false);
    }
  }

  // ── Unassign vehicle ──────────────────────────────────────────────────────

  protected async unassignVehicle(routeCode: string, vehicleId: string): Promise<void> {
    this.unassigningKey.set(`${routeCode}:${vehicleId}`);
    try {
      const detail = await this.routeService.unassignVehicle(routeCode, vehicleId);
      this.routes.update(list => list.map(r =>
        r.route.routeCode === routeCode
          ? { ...r, route: detail.route, assignedVehicles: [...detail.assignedVehicles] }
          : r
      ));
    } catch {
      this.error.set('Failed to unassign vehicle.');
    } finally {
      this.unassigningKey.set(null);
    }
  }
}
