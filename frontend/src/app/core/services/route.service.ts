import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  RouteAssignmentResponse,
  RouteDetailResponse,
  CreateRouteRequest,
  UpdateRouteRequest
} from '../models/route.models';
import { PagedResponse } from '../models/paged-response';

@Injectable({ providedIn: 'root' })
export class RouteService {
  private readonly http = inject(HttpClient);

  getRoutes(page = 1, pageSize = 20): Promise<PagedResponse<RouteAssignmentResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return firstValueFrom(
      this.http.get<PagedResponse<RouteAssignmentResponse>>('/api/routes', { params })
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
