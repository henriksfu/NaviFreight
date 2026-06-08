import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { YardService } from '../../core/services/yard.service';
import { YardResponse } from '../../core/models/yard.models';

@Component({
  selector: 'app-yards-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './yards-page.component.html',
  styleUrl: './yards-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class YardsPageComponent implements OnInit {
  private readonly yardService = inject(YardService);
  private readonly authService = inject(AuthService);

  protected readonly isAdmin = computed(() => this.authService.isTenantAdmin());
  protected readonly isDispatch = computed(() => {
    const role = this.authService.userRole();
    return role === 'Tenant Admin' || role === 'Dispatcher';
  });

  protected readonly yards = signal<YardResponse[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Create yard form
  protected readonly showCreateForm = signal(false);
  protected createName = '';
  protected createCapacity = 100;
  protected readonly isSaving = signal(false);
  protected readonly formError = signal<string | null>(null);

  // Edit yard (inline per-card)
  protected readonly editingYardId = signal<number | null>(null);
  protected editName = '';
  protected editCapacity = 0;

  // Operational update (inline per-card)
  protected readonly updatingYardId = signal<number | null>(null);
  protected opOccupied = 0;
  protected opInbound = 0;
  protected opTurn = 0;

  protected readonly deletingYardId = signal<number | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected occupancyPct(yard: YardResponse): number {
    return yard.capacity > 0
      ? Math.min(100, Math.round((yard.occupiedSlots / yard.capacity) * 100))
      : 0;
  }

  protected trackByYardId(_: number, yard: YardResponse): number {
    return yard.yardId;
  }

  protected openCreate(): void {
    this.createName = '';
    this.createCapacity = 100;
    this.formError.set(null);
    this.showCreateForm.set(true);
  }

  protected openEdit(yard: YardResponse): void {
    this.editName = yard.yardName;
    this.editCapacity = yard.capacity;
    this.formError.set(null);
    this.editingYardId.set(yard.yardId);
  }

  protected openOperational(yard: YardResponse): void {
    this.opOccupied = yard.occupiedSlots;
    this.opInbound = yard.inboundQueue;
    this.opTurn = yard.averageTurnMinutes;
    this.updatingYardId.set(yard.yardId);
  }

  protected cancelCreate(): void {
    this.showCreateForm.set(false);
    this.formError.set(null);
  }

  protected cancelEdit(): void {
    this.editingYardId.set(null);
    this.formError.set(null);
  }

  protected cancelOp(): void {
    this.updatingYardId.set(null);
  }

  protected async submitCreate(): Promise<void> {
    if (!this.createName.trim()) {
      this.formError.set('Yard name is required.');
      return;
    }
    if (this.createCapacity <= 0) {
      this.formError.set('Capacity must be greater than zero.');
      return;
    }
    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const result = await this.yardService.createYard({
        yardName: this.createName.trim(),
        capacity: this.createCapacity
      });
      this.yards.update(list => [result.yard, ...list]);
      this.showCreateForm.set(false);
    } catch {
      this.formError.set('Failed to create yard. Please try again.');
    } finally {
      this.isSaving.set(false);
    }
  }

  protected async submitEdit(yardId: number): Promise<void> {
    if (!this.editName.trim()) {
      this.formError.set('Yard name is required.');
      return;
    }
    this.isSaving.set(true);
    this.formError.set(null);
    try {
      const updated = await this.yardService.updateYard(yardId, {
        yardName: this.editName.trim(),
        capacity: this.editCapacity
      });
      this.yards.update(list => list.map(y => y.yardId === yardId ? updated : y));
      this.editingYardId.set(null);
    } catch {
      this.formError.set('Failed to update yard.');
    } finally {
      this.isSaving.set(false);
    }
  }

  protected async submitOperational(yardId: number): Promise<void> {
    this.isSaving.set(true);
    try {
      const updated = await this.yardService.updateOperational(yardId, {
        occupiedSlots: this.opOccupied,
        inboundQueue: this.opInbound,
        averageTurnMinutes: this.opTurn
      });
      this.yards.update(list => list.map(y => y.yardId === yardId ? updated : y));
      this.updatingYardId.set(null);
    } catch {
      this.error.set('Failed to update operational data.');
    } finally {
      this.isSaving.set(false);
    }
  }

  protected async deleteYard(yardId: number): Promise<void> {
    this.deletingYardId.set(yardId);
    try {
      await this.yardService.deleteYard(yardId);
      this.yards.update(list => list.filter(y => y.yardId !== yardId));
    } catch {
      this.error.set('Failed to delete yard.');
    } finally {
      this.deletingYardId.set(null);
    }
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const result = await this.yardService.getYards();
      this.yards.set(result.items);
    } catch {
      this.error.set('Failed to load yards. Check that the backend is running.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
