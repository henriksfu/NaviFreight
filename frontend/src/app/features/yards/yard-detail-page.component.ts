import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, numberAttribute, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { YardService } from '../../core/services/yard.service';
import { DockResponse, YardResponse } from '../../core/models/yard.models';

@Component({
  selector: 'app-yard-detail-page',
  standalone: true,
  imports: [RouterLink, NgClass, FormsModule],
  templateUrl: './yard-detail-page.component.html',
  styleUrl: './yard-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class YardDetailPageComponent implements OnInit {
  readonly yardId = input.required<number, string>({ transform: numberAttribute });

  private readonly yardService = inject(YardService);
  private readonly authService = inject(AuthService);

  protected readonly isAdmin = computed(() => this.authService.isTenantAdmin());
  protected readonly isDispatch = computed(() => {
    const role = this.authService.userRole();
    return role === 'Tenant Admin' || role === 'Dispatcher';
  });

  protected readonly yard = signal<YardResponse | null>(null);
  protected readonly docks = signal<DockResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Operational update
  protected readonly showOpForm = signal(false);
  protected opOccupied = 0;
  protected opInbound = 0;
  protected opTurn = 0;
  protected readonly isSavingOp = signal(false);

  // Create dock form
  protected readonly showCreateDock = signal(false);
  protected newDockCode = '';
  protected newDockStatus = 'Available';
  protected readonly isSavingDock = signal(false);
  protected readonly dockFormError = signal<string | null>(null);

  // Inline edit dock
  protected readonly editingDockId = signal<number | null>(null);
  protected editDockCode = '';
  protected editDockStatus = 'Available';
  protected editDockNotes = '';
  protected readonly isSavingDockEdit = signal(false);

  // Assign vehicle to dock
  protected readonly assigningDockId = signal<number | null>(null);
  protected assignVehicleId = '';
  protected readonly isSavingAssign = signal(false);
  protected readonly assignError = signal<string | null>(null);

  protected readonly deletingDockId = signal<number | null>(null);
  protected readonly releasingDockId = signal<number | null>(null);

  readonly dockStatuses = ['Available', 'Occupied', 'Maintenance', 'Offline'] as const;

  ngOnInit(): void {
    this.load();
  }

  protected trackByDockId(_: number, dock: DockResponse): number {
    return dock.dockId;
  }

  protected statusClass(status: string): string {
    return {
      Available: 'status-available',
      Occupied: 'status-occupied',
      Maintenance: 'status-warn',
      Offline: 'status-offline'
    }[status] ?? '';
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const detail = await this.yardService.getYardDetail(this.yardId());
      this.yard.set(detail.yard);
      this.docks.set(detail.docks);
    } catch {
      this.error.set('Failed to load yard details. Check that the backend is running.');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ── Operational update ────────────────────────────────────────────────────

  protected openOpForm(): void {
    const y = this.yard();
    if (!y) return;
    this.opOccupied = y.occupiedSlots;
    this.opInbound = y.inboundQueue;
    this.opTurn = y.averageTurnMinutes;
    this.showOpForm.set(true);
  }

  protected cancelOp(): void {
    this.showOpForm.set(false);
  }

  protected async submitOp(): Promise<void> {
    this.isSavingOp.set(true);
    try {
      const updated = await this.yardService.updateOperational(this.yardId(), {
        occupiedSlots: this.opOccupied,
        inboundQueue: this.opInbound,
        averageTurnMinutes: this.opTurn
      });
      this.yard.set(updated);
      this.showOpForm.set(false);
    } catch {
      this.error.set('Failed to update operational data.');
    } finally {
      this.isSavingOp.set(false);
    }
  }

  // ── Create dock ───────────────────────────────────────────────────────────

  protected openCreateDock(): void {
    this.newDockCode = '';
    this.newDockStatus = 'Available';
    this.dockFormError.set(null);
    this.showCreateDock.set(true);
  }

  protected cancelCreateDock(): void {
    this.showCreateDock.set(false);
    this.dockFormError.set(null);
  }

  protected async submitCreateDock(): Promise<void> {
    if (!this.newDockCode.trim()) {
      this.dockFormError.set('Dock code is required.');
      return;
    }
    this.isSavingDock.set(true);
    this.dockFormError.set(null);
    try {
      const dock = await this.yardService.createDock(this.yardId(), {
        dockCode: this.newDockCode.trim(),
        status: this.newDockStatus
      });
      this.docks.update(list => [...list, dock]);
      this.showCreateDock.set(false);
    } catch {
      this.dockFormError.set('Failed to create dock.');
    } finally {
      this.isSavingDock.set(false);
    }
  }

  // ── Edit dock ─────────────────────────────────────────────────────────────

  protected openEditDock(dock: DockResponse): void {
    this.editDockCode = dock.dockCode;
    this.editDockStatus = dock.status;
    this.editDockNotes = dock.notes ?? '';
    this.editingDockId.set(dock.dockId);
  }

  protected cancelEditDock(): void {
    this.editingDockId.set(null);
  }

  protected async submitEditDock(dockId: number): Promise<void> {
    if (!this.editDockCode.trim()) return;
    this.isSavingDockEdit.set(true);
    try {
      const updated = await this.yardService.updateDock(this.yardId(), dockId, {
        dockCode: this.editDockCode.trim(),
        status: this.editDockStatus,
        notes: this.editDockNotes.trim() || null
      });
      this.docks.update(list => list.map(d => d.dockId === dockId ? updated : d));
      this.editingDockId.set(null);
    } catch {
      this.error.set('Failed to update dock.');
    } finally {
      this.isSavingDockEdit.set(false);
    }
  }

  // ── Delete dock ───────────────────────────────────────────────────────────

  protected async deleteDock(dockId: number): Promise<void> {
    this.deletingDockId.set(dockId);
    try {
      await this.yardService.deleteDock(this.yardId(), dockId);
      this.docks.update(list => list.filter(d => d.dockId !== dockId));
    } catch {
      this.error.set('Failed to delete dock.');
    } finally {
      this.deletingDockId.set(null);
    }
  }

  // ── Assign vehicle ────────────────────────────────────────────────────────

  protected openAssign(dockId: number): void {
    this.assignVehicleId = '';
    this.assignError.set(null);
    this.assigningDockId.set(dockId);
  }

  protected cancelAssign(): void {
    this.assigningDockId.set(null);
    this.assignError.set(null);
  }

  protected async submitAssign(dockId: number): Promise<void> {
    if (!this.assignVehicleId.trim()) {
      this.assignError.set('Vehicle ID is required.');
      return;
    }
    this.isSavingAssign.set(true);
    this.assignError.set(null);
    try {
      const updated = await this.yardService.assignVehicle(
        this.yardId(), dockId, this.assignVehicleId.trim()
      );
      this.docks.update(list => list.map(d => d.dockId === dockId ? updated : d));
      this.assigningDockId.set(null);
    } catch {
      this.assignError.set('Failed to assign vehicle. Check the vehicle ID.');
    } finally {
      this.isSavingAssign.set(false);
    }
  }

  // ── Release dock ──────────────────────────────────────────────────────────

  protected async releaseDock(dockId: number): Promise<void> {
    this.releasingDockId.set(dockId);
    try {
      const updated = await this.yardService.releaseDock(this.yardId(), dockId);
      this.docks.update(list => list.map(d => d.dockId === dockId ? updated : d));
    } catch {
      this.error.set('Failed to release dock.');
    } finally {
      this.releasingDockId.set(null);
    }
  }
}
