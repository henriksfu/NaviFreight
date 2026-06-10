import {
  ChangeDetectionStrategy, Component, inject, OnInit, signal, computed
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass } from '@angular/common';
import { UserService } from '../../core/services/user.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { UserResponse } from '../../core/models/user.models';

const ROLES = ['Tenant Admin', 'Dispatcher', 'Yard Manager'] as const;

interface EditState {
  displayName: string;
  role: string;
  newPassword: string;
}

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersPageComponent implements OnInit {
  private readonly userService  = inject(UserService);
  private readonly toast        = inject(ToastService);
  protected readonly authService = inject(AuthService);

  protected readonly roles = ROLES;

  protected readonly users      = signal<UserResponse[]>([]);
  protected readonly isLoading  = signal(true);
  protected readonly error      = signal<string | null>(null);
  protected readonly showCreate = signal(false);
  protected readonly editingId  = signal<number | null>(null);
  protected readonly savingId   = signal<number | null>(null);

  protected readonly activeCount   = computed(() => this.users().filter(u => u.isActive).length);
  protected readonly inactiveCount = computed(() => this.users().filter(u => !u.isActive).length);

  // Create form
  protected newEmail       = '';
  protected newDisplayName = '';
  protected newRole        = 'Dispatcher';
  protected newPassword    = '';
  protected creating       = false;

  // Edit form state keyed by userId
  protected readonly editState: Record<number, EditState> = {};

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const users = await this.userService.getUsers();
      this.users.set(users);
    } catch {
      this.error.set('Failed to load users.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openCreate(): void {
    this.newEmail = '';
    this.newDisplayName = '';
    this.newRole = 'Dispatcher';
    this.newPassword = '';
    this.showCreate.set(true);
  }

  protected cancelCreate(): void {
    this.showCreate.set(false);
  }

  protected async submitCreate(): Promise<void> {
    if (!this.newEmail || !this.newDisplayName || !this.newPassword) return;
    this.creating = true;
    try {
      const created = await this.userService.createUser({
        email: this.newEmail,
        displayName: this.newDisplayName,
        role: this.newRole,
        password: this.newPassword
      });
      this.users.update(u => [...u, created]);
      this.showCreate.set(false);
      this.toast.success(`${created.displayName} added successfully.`);
    } catch {
      // error interceptor handles toast
    } finally {
      this.creating = false;
    }
  }

  protected openEdit(user: UserResponse): void {
    this.editState[user.userId] = {
      displayName: user.displayName,
      role: user.role,
      newPassword: ''
    };
    this.editingId.set(user.userId);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected async submitEdit(userId: number): Promise<void> {
    const state = this.editState[userId];
    if (!state) return;
    this.savingId.set(userId);
    try {
      const updated = await this.userService.updateUser(userId, {
        displayName: state.displayName,
        role: state.role,
        newPassword: state.newPassword || undefined
      });
      this.users.update(u => u.map(x => x.userId === userId ? updated : x));
      this.editingId.set(null);
      this.toast.success('User updated.');
    } catch {
      // error interceptor handles toast
    } finally {
      this.savingId.set(null);
    }
  }

  protected async deactivate(user: UserResponse): Promise<void> {
    this.savingId.set(user.userId);
    try {
      await this.userService.deactivateUser(user.userId);
      this.users.update(u => u.map(x => x.userId === user.userId ? { ...x, isActive: false } : x));
      this.toast.warn(`${user.displayName} has been deactivated.`);
    } catch {
      // error interceptor handles toast
    } finally {
      this.savingId.set(null);
    }
  }

  protected async reactivate(user: UserResponse): Promise<void> {
    this.savingId.set(user.userId);
    try {
      const updated = await this.userService.reactivateUser(user.userId);
      this.users.update(u => u.map(x => x.userId === user.userId ? updated : x));
      this.toast.success(`${updated.displayName} has been reactivated.`);
    } catch {
      // error interceptor handles toast
    } finally {
      this.savingId.set(null);
    }
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
