import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { UserResponse, CreateUserRequest, UpdateUserRequest } from '../models/user.models';
import { PagedResponse } from '../models/paged-response';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/users';

  getUsers(page = 1, pageSize = 20): Promise<PagedResponse<UserResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return firstValueFrom(this.http.get<PagedResponse<UserResponse>>(this.base, { params }));
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
