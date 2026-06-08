import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  RouteAssignmentResponse,
  RouteDetailResponse,
  CreateRouteRequest,
  UpdateRouteRequest
} from '../models/route.models';

@Injectable({ providedIn: 'root' })
export class RouteService {
  private readonly http = inject(HttpClient);

  getRoutes(): Promise<{ tenantId: string; items: RouteAssignmentResponse[] }> {
    return firstValueFrom(
      this.http.get<{ tenantId: string; items: RouteAssignmentResponse[] }>('/api/routes')
    );
  }

  getRoute(routeCode: string): Promise<RouteDetailResponse> {
    return firstValueFrom(this.http.get<RouteDetailResponse>(`/api/routes/${routeCode}`));
  }

  createRoute(request: CreateRouteRequest): Promise<RouteAssignmentResponse> {
    return firstValueFrom(
      this.http.post<RouteAssignmentResponse>('/api/routes', request)
    );
  }

  updateRoute(routeCode: string, request: UpdateRouteRequest): Promise<RouteAssignmentResponse> {
    return firstValueFrom(
      this.http.put<RouteAssignmentResponse>(`/api/routes/${routeCode}`, request)
    );
  }

  deleteRoute(routeCode: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/routes/${routeCode}`));
  }

  assignVehicles(routeCode: string, vehicleIds: string[]): Promise<RouteDetailResponse> {
    return firstValueFrom(
      this.http.post<RouteDetailResponse>(`/api/routes/${routeCode}/assignments`, { vehicleIds })
    );
  }

  unassignVehicle(routeCode: string, vehicleId: string): Promise<RouteDetailResponse> {
    return firstValueFrom(
      this.http.delete<RouteDetailResponse>(`/api/routes/${routeCode}/assignments/${vehicleId}`)
    );
  }
}
