import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { FleetService } from '../../core/services/fleet.service';
import { FleetVehicleResponse, DriverResponse } from '../../core/models/fleet.models';
import { PaginationComponent } from '../../shared/components/pagination.component';

type Tab = 'vehicles' | 'drivers';

@Component({
  selector: 'app-fleet-page',
  standalone: true,
  imports: [NgClass, FormsModule, PaginationComponent],
  templateUrl: './fleet-page.component.html',
  styleUrl: './fleet-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FleetPageComponent implements OnInit {
  private readonly fleetService = inject(FleetService);
  private readonly authService = inject(AuthService);

  protected readonly isAdmin = computed(() => this.authService.isTenantAdmin());
  protected readonly isDispatch = computed(() => {
    const role = this.authService.userRole();
    return role === 'Tenant Admin' || role === 'Dispatcher';
  });

  protected readonly activeTab = signal<Tab>('vehicles');

  // Vehicles state + pagination
  protected readonly vehicles          = signal<FleetVehicleResponse[]>([]);
  protected readonly isLoadingVehicles = signal(true);
  protected vehiclePage     = 1;
  protected vehicleTotalPages = 1;
  protected readonly pageSize = 20;

  // Drivers state + pagination
  protected readonly drivers          = signal<DriverResponse[]>([]);
  protected readonly isLoadingDrivers = signal(false);
  protected readonly driversLoaded    = signal(false);
  protected driverPage      = 1;
  protected driverTotalPages = 1;

  protected readonly error = signal<string | null>(null);

  // Vehicle metrics
  protected readonly averageUtilization = computed(() => {
    const v = this.vehicles();
    return v.length === 0 ? 0 : Math.round(v.reduce((s, x) => s + x.utilizationPercent, 0) / v.length);
  });

  // Create vehicle form
  protected readonly showCreateVehicle = signal(false);
  protected newVehicleId = '';
  protected newVehicleStatus = 'Awaiting Dispatch';
  protected newVehicleUtil = 0;
  protected readonly isSavingVehicle = signal(false);
  protected readonly vehicleFormError = signal<string | null>(null);

  // Edit vehicle inline
  protected readonly editingVehicleId = signal<string | null>(null);
  protected editVehicleStatus = '';
  protected editVehicleUtil = 0;
  protected readonly isSavingVehicleEdit = signal(false);

  // Assign driver inline
  protected readonly assigningDriverVehicleId = signal<string | null>(null);
  protected assignDriverId = '';
  protected readonly isSavingAssign = signal(false);
  protected readonly assignError = signal<string | null>(null);

  protected readonly deletingVehicleId = signal<string | null>(null);

  // Create driver form
  protected readonly showCreateDriver = signal(false);
  protected newDriverName = '';
  protected newDriverLicense = '';
  protected newDriverStatus = 'Available';
  protected readonly isSavingDriver = signal(false);
  protected readonly driverFormError = signal<string | null>(null);

  // Edit driver inline
  protected readonly editingDriverId = signal<number | null>(null);
  protected editDriverName = '';
  protected editDriverLicense = '';
  protected editDriverStatus = '';
  protected readonly isSavingDriverEdit = signal(false);

  protected readonly deletingDriverId = signal<number | null>(null);

  readonly vehicleStatuses = ['In Transit', 'At Dock', 'Awaiting Dispatch', 'Delayed', 'Available'];
  readonly driverStatuses = ['Available', 'On Duty', 'Off Duty', 'On Leave'];

  ngOnInit(): void {
    this.loadVehicles();
  }

  protected switchTab(tab: Tab): void {
    this.activeTab.set(tab);
    if (tab === 'drivers' && !this.driversLoaded()) {
      this.loadDrivers();
    }
  }

  protected trackByVehicleId(_: number, v: FleetVehicleResponse): string {
    return v.vehicleId;
  }

  protected trackByDriverId(_: number, d: DriverResponse): number {
    return d.driverId;
  }

  protected statusClass(status: string): string {
    return 's-' + status.toLowerCase().replace(/\s+/g, '-');
  }

  protected driverStatusClass(status: string): string {
    return 'ds-' + status.toLowerCase().replace(/\s+/g, '-');
  }

  protected formatDate(utc: string | null): string {
    if (!utc) return '—';
    return new Date(utc).toLocaleString(undefined, {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  // ── Load ──────────────────────────────────────────────────────────────────

  protected async onVehiclePage(page: number): Promise<void> {
    this.vehiclePage = page;
    await this.loadVehicles();
  }

  protected async onDriverPage(page: number): Promise<void> {
    this.driverPage = page;
    await this.loadDrivers();
  }

  private async loadVehicles(): Promise<void> {
    this.isLoadingVehicles.set(true);
    this.error.set(null);
    try {
      const result = await this.fleetService.getVehicles(this.vehiclePage, this.pageSize);
      this.vehicles.set(result.items);
      this.vehicleTotalPages = result.totalPages;
    } catch {
      this.error.set('Failed to load fleet. Check that the backend is running.');
    } finally {
      this.isLoadingVehicles.set(false);
    }
  }

  private async loadDrivers(): Promise<void> {
    this.isLoadingDrivers.set(true);
    try {
      const result = await this.fleetService.getDrivers(this.driverPage, this.pageSize);
      this.drivers.set(result.items);
      this.driverTotalPages = result.totalPages;
      this.driversLoaded.set(true);
    } catch {
      this.error.set('Failed to load drivers.');
    } finally {
      this.isLoadingDrivers.set(false);
    }
  }

  // ── Create vehicle ────────────────────────────────────────────────────────

  protected openCreateVehicle(): void {
    this.newVehicleId = '';
    this.newVehicleStatus = 'Awaiting Dispatch';
    this.newVehicleUtil = 0;
    this.vehicleFormError.set(null);
    this.showCreateVehicle.set(true);
  }

  protected cancelCreateVehicle(): void {
    this.showCreateVehicle.set(false);
    this.vehicleFormError.set(null);
  }

  protected async submitCreateVehicle(): Promise<void> {
    if (!this.newVehicleId.trim()) { this.vehicleFormError.set('Vehicle ID is required.'); return; }
    this.isSavingVehicle.set(true);
    this.vehicleFormError.set(null);
    try {
      const created = await this.fleetService.createVehicle({
        vehicleId: this.newVehicleId.trim(),
        status: this.newVehicleStatus,
        currentYardId: null,
        etaUtc: null,
        utilizationPercent: this.newVehicleUtil
      });
      const summary: FleetVehicleResponse = {
        vehicleId: created.vehicleId,
        driverName: created.driverName,
        status: created.status,
        currentYard: created.currentYard,
        lastUpdatedUtc: created.lastUpdatedUtc,
        etaUtc: created.etaUtc,
        utilizationPercent: created.utilizationPercent,
        routeCode: created.routeCode
      };
      this.vehicles.update(list => [summary, ...list]);
      this.showCreateVehicle.set(false);
    } catch {
      this.vehicleFormError.set('Failed to create vehicle. The ID may already exist.');
    } finally {
      this.isSavingVehicle.set(false);
    }
  }

  // ── Edit vehicle ──────────────────────────────────────────────────────────

  protected openEditVehicle(v: FleetVehicleResponse): void {
    this.editVehicleStatus = v.status;
    this.editVehicleUtil = v.utilizationPercent;
    this.editingVehicleId.set(v.vehicleId);
  }

  protected cancelEditVehicle(): void {
    this.editingVehicleId.set(null);
  }

  protected async submitEditVehicle(vehicleId: string): Promise<void> {
    this.isSavingVehicleEdit.set(true);
    try {
      const updated = await this.fleetService.updateVehicle(vehicleId, {
        status: this.editVehicleStatus,
        currentYardId: null,
        etaUtc: null,
        utilizationPercent: this.editVehicleUtil
      });
      this.vehicles.update(list => list.map(v =>
        v.vehicleId === vehicleId
          ? { ...v, status: updated.status, utilizationPercent: updated.utilizationPercent, lastUpdatedUtc: updated.lastUpdatedUtc }
          : v
      ));
      this.editingVehicleId.set(null);
    } catch {
      this.error.set('Failed to update vehicle.');
    } finally {
      this.isSavingVehicleEdit.set(false);
    }
  }

  // ── Delete vehicle ────────────────────────────────────────────────────────

  protected async deleteVehicle(vehicleId: string): Promise<void> {
    this.deletingVehicleId.set(vehicleId);
    try {
      await this.fleetService.deleteVehicle(vehicleId);
      this.vehicles.update(list => list.filter(v => v.vehicleId !== vehicleId));
    } catch {
      this.error.set('Failed to delete vehicle.');
    } finally {
      this.deletingVehicleId.set(null);
    }
  }

  // ── Assign driver ─────────────────────────────────────────────────────────

  protected openAssignDriver(vehicleId: string): void {
    this.assignDriverId = '';
    this.assignError.set(null);
    this.assigningDriverVehicleId.set(vehicleId);
  }

  protected cancelAssignDriver(): void {
    this.assigningDriverVehicleId.set(null);
    this.assignError.set(null);
  }

  protected async submitAssignDriver(vehicleId: string): Promise<void> {
    const driverId = parseInt(this.assignDriverId, 10);
    if (isNaN(driverId) || driverId <= 0) {
      this.assignError.set('Enter a valid numeric driver ID.');
      return;
    }
    this.isSavingAssign.set(true);
    this.assignError.set(null);
    try {
      const updated = await this.fleetService.assignDriver(vehicleId, driverId);
      this.vehicles.update(list => list.map(v =>
        v.vehicleId === vehicleId ? { ...v, driverName: updated.driverName } : v
      ));
      this.assigningDriverVehicleId.set(null);
    } catch {
      this.assignError.set('Failed to assign driver. Check the driver ID.');
    } finally {
      this.isSavingAssign.set(false);
    }
  }

  protected async unassignDriver(vehicleId: string): Promise<void> {
    try {
      const updated = await this.fleetService.unassignDriver(vehicleId);
      this.vehicles.update(list => list.map(v =>
        v.vehicleId === vehicleId ? { ...v, driverName: updated.driverName } : v
      ));
    } catch {
      this.error.set('Failed to unassign driver.');
    }
  }

  // ── Create driver ─────────────────────────────────────────────────────────

  protected openCreateDriver(): void {
    this.newDriverName = '';
    this.newDriverLicense = '';
    this.newDriverStatus = 'Available';
    this.driverFormError.set(null);
    this.showCreateDriver.set(true);
  }

  protected cancelCreateDriver(): void {
    this.showCreateDriver.set(false);
    this.driverFormError.set(null);
  }

  protected async submitCreateDriver(): Promise<void> {
    if (!this.newDriverName.trim()) { this.driverFormError.set('Name is required.'); return; }
    if (!this.newDriverLicense.trim()) { this.driverFormError.set('License number is required.'); return; }
    this.isSavingDriver.set(true);
    this.driverFormError.set(null);
    try {
      const created = await this.fleetService.createDriver({
        fullName: this.newDriverName.trim(),
        licenseNumber: this.newDriverLicense.trim(),
        availabilityStatus: this.newDriverStatus
      });
      this.drivers.update(list => [...list, created]);
      this.showCreateDriver.set(false);
    } catch {
      this.driverFormError.set('Failed to create driver.');
    } finally {
      this.isSavingDriver.set(false);
    }
  }

  // ── Edit driver ───────────────────────────────────────────────────────────

  protected openEditDriver(d: DriverResponse): void {
    this.editDriverName = d.fullName;
    this.editDriverLicense = d.licenseNumber;
    this.editDriverStatus = d.availabilityStatus;
    this.editingDriverId.set(d.driverId);
  }

  protected cancelEditDriver(): void {
    this.editingDriverId.set(null);
  }

  protected async submitEditDriver(driverId: number): Promise<void> {
    if (!this.editDriverName.trim()) return;
    this.isSavingDriverEdit.set(true);
    try {
      const updated = await this.fleetService.updateDriver(driverId, {
        fullName: this.editDriverName.trim(),
        licenseNumber: this.editDriverLicense.trim(),
        availabilityStatus: this.editDriverStatus
      });
      this.drivers.update(list => list.map(d => d.driverId === driverId ? updated : d));
      this.editingDriverId.set(null);
    } catch {
      this.error.set('Failed to update driver.');
    } finally {
      this.isSavingDriverEdit.set(false);
    }
  }

  // ── Delete driver ─────────────────────────────────────────────────────────

  protected async deleteDriver(driverId: number): Promise<void> {
    this.deletingDriverId.set(driverId);
    try {
      await this.fleetService.deleteDriver(driverId);
      this.drivers.update(list => list.filter(d => d.driverId !== driverId));
    } catch {
      this.error.set('Failed to delete driver.');
    } finally {
      this.deletingDriverId.set(null);
    }
  }
}
