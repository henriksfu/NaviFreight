import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { LoginRequest, LoginResponse, UserProfile } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private static readonly TOKEN_KEY = 'nf_token';
  private static readonly USER_KEY = 'nf_user';

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _currentUser = signal<UserProfile | null>(AuthService.loadStoredUser());

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly userRole = computed(() => this._currentUser()?.role ?? null);
  readonly isTenantAdmin = computed(() => this._currentUser()?.role === 'Tenant Admin');

  async login(request: LoginRequest): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/auth/login', request)
    );
    localStorage.setItem(AuthService.TOKEN_KEY, response.token);
    localStorage.setItem(AuthService.USER_KEY, JSON.stringify(response.user));
    this._currentUser.set(response.user);
    await this.router.navigate(['/']);
  }

  logout(): void {
    localStorage.removeItem(AuthService.TOKEN_KEY);
    localStorage.removeItem(AuthService.USER_KEY);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(AuthService.TOKEN_KEY);
  }

  private static loadStoredUser(): UserProfile | null {
    try {
      const stored = localStorage.getItem(AuthService.USER_KEY);
      return stored ? (JSON.parse(stored) as UserProfile) : null;
    } catch {
      return null;
    }
  }
}
