import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  YardResponse, YardDetailResponse, DockResponse,
  CreateYardRequest, UpdateYardRequest, YardOperationalUpdateRequest,
  CreateDockRequest, UpdateDockRequest
} from '../models/yard.models';

@Injectable({ providedIn: 'root' })
export class YardService {
  private readonly http = inject(HttpClient);

  getYards(): Promise<{ tenantId: string; items: YardResponse[] }> {
    return firstValueFrom(
      this.http.get<{ tenantId: string; items: YardResponse[] }>('/api/yards')
    );
  }

  getYardDetail(yardId: number): Promise<YardDetailResponse> {
    return firstValueFrom(
      this.http.get<YardDetailResponse>(`/api/yards/${yardId}`)
    );
  }

  createYard(request: CreateYardRequest): Promise<YardDetailResponse> {
    return firstValueFrom(
      this.http.post<YardDetailResponse>('/api/yards', request)
    );
  }

  updateYard(yardId: number, request: UpdateYardRequest): Promise<YardResponse> {
    return firstValueFrom(
      this.http.put<YardResponse>(`/api/yards/${yardId}`, request)
    );
  }

  deleteYard(yardId: number): Promise<void> {
    return firstValueFrom(
      this.http.delete<void>(`/api/yards/${yardId}`)
    );
  }

  updateOperational(yardId: number, request: YardOperationalUpdateRequest): Promise<YardResponse> {
    return firstValueFrom(
      this.http.patch<YardResponse>(`/api/yards/${yardId}/operational`, request)
    );
  }

  getDocks(yardId: number): Promise<{ yardId: number; items: DockResponse[] }> {
    return firstValueFrom(
      this.http.get<{ yardId: number; items: DockResponse[] }>(`/api/yards/${yardId}/docks`)
    );
  }

  createDock(yardId: number, request: CreateDockRequest): Promise<DockResponse> {
    return firstValueFrom(
      this.http.post<DockResponse>(`/api/yards/${yardId}/docks`, request)
    );
  }

  updateDock(yardId: number, dockId: number, request: UpdateDockRequest): Promise<DockResponse> {
    return firstValueFrom(
      this.http.put<DockResponse>(`/api/yards/${yardId}/docks/${dockId}`, request)
    );
  }

  deleteDock(yardId: number, dockId: number): Promise<void> {
    return firstValueFrom(
      this.http.delete<void>(`/api/yards/${yardId}/docks/${dockId}`)
    );
  }

  assignVehicle(yardId: number, dockId: number, vehicleId: string): Promise<DockResponse> {
    return firstValueFrom(
      this.http.post<DockResponse>(
        `/api/yards/${yardId}/docks/${dockId}/vehicle`,
        { vehicleId }
      )
    );
  }

  releaseDock(yardId: number, dockId: number): Promise<DockResponse> {
    return firstValueFrom(
      this.http.delete<DockResponse>(`/api/yards/${yardId}/docks/${dockId}/vehicle`)
    );
  }
}
