import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { UserResponse, CreateUserRequest, UpdateUserRequest } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/users';

  getUsers(): Promise<UserResponse[]> {
    return firstValueFrom(this.http.get<UserResponse[]>(this.base));
  }

  createUser(req: CreateUserRequest): Promise<UserResponse> {
    return firstValueFrom(this.http.post<UserResponse>(this.base, req));
  }

  updateUser(userId: number, req: UpdateUserRequest): Promise<UserResponse> {
    return firstValueFrom(this.http.put<UserResponse>(`${this.base}/${userId}`, req));
  }

  deactivateUser(userId: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.base}/${userId}`));
  }

  reactivateUser(userId: number): Promise<UserResponse> {
    return firstValueFrom(this.http.post<UserResponse>(`${this.base}/${userId}/reactivate`, {}));
  }
}
