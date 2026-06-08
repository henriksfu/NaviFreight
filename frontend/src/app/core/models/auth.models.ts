export type UserRole = 'Tenant Admin' | 'Dispatcher' | 'Yard Manager';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserProfile {
  userId: number;
  email: string;
  displayName: string;
  role: UserRole;
  tenantId: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserProfile;
}
