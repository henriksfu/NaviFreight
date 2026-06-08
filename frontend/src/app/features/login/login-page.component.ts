import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPageComponent {
  private readonly authService = inject(AuthService);

  protected email = '';
  protected password = '';
  protected readonly isLoading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async onSubmit(): Promise<void> {
    if (!this.email || !this.password) return;

    this.isLoading.set(true);
    this.error.set(null);

    try {
      await this.authService.login({ email: this.email, password: this.password });
    } catch {
      this.error.set('Invalid email or password. Please try again.');
      this.isLoading.set(false);
    }
  }
}
