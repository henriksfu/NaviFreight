import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  FleetVehicleResponse,
  FleetVehicleDetailResponse,
  DriverResponse,
  CreateVehicleRequest,
  UpdateVehicleRequest,
  CreateDriverRequest,
  UpdateDriverRequest
} from '../models/fleet.models';
import { PagedResponse } from '../models/paged-response';

@Injectable({ providedIn: 'root' })
export class FleetService {
  private readonly http = inject(HttpClient);

  // ── Vehicles ──────────────────────────────────────────────────────────────

  getVehicles(page = 1, pageSize = 20): Promise<PagedResponse<FleetVehicleResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return firstValueFrom(
      this.http.get<PagedResponse<FleetVehicleResponse>>('/api/fleet/vehicles', { params })
    );
  }

  getVehicle(vehicleId: string): Promise<FleetVehicleDetailResponse> {
    return firstValueFrom(
      this.http.get<FleetVehicleDetailResponse>(`/api/fleet/vehicles/${vehicleId}`)
    );
  }

  createVehicle(request: CreateVehicleRequest): Promise<FleetVehicleDetailResponse> {
    return firstValueFrom(
      this.http.post<FleetVehicleDetailResponse>('/api/fleet/vehicles', request)
    );
  }

  updateVehicle(vehicleId: string, request: UpdateVehicleRequest): Promise<FleetVehicleDetailResponse> {
    return firstValueFrom(
      this.http.put<FleetVehicleDetailResponse>(`/api/fleet/vehicles/${vehicleId}`, request)
    );
  }

  deleteVehicle(vehicleId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/fleet/vehicles/${vehicleId}`));
  }

  assignDriver(vehicleId: string, driverId: number): Promise<FleetVehicleDetailResponse> {
    return firstValueFrom(
      this.http.post<FleetVehicleDetailResponse>(`/api/fleet/vehicles/${vehicleId}/driver`, { driverId })
    );
  }

  unassignDriver(vehicleId: string): Promise<FleetVehicleDetailResponse> {
    return firstValueFrom(
      this.http.delete<FleetVehicleDetailResponse>(`/api/fleet/vehicles/${vehicleId}/driver`)
    );
  }

  // ── Drivers ───────────────────────────────────────────────────────────────

  getDrivers(page = 1, pageSize = 20): Promise<PagedResponse<DriverResponse>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return firstValueFrom(
      this.http.get<PagedResponse<DriverResponse>>('/api/fleet/drivers', { params })
    );
  }

  getDriver(driverId: number): Promise<DriverResponse> {
    return firstValueFrom(
      this.http.get<DriverResponse>(`/api/fleet/drivers/${driverId}`)
    );
  }

  createDriver(request: CreateDriverRequest): Promise<DriverResponse> {
    return firstValueFrom(
      this.http.post<DriverResponse>('/api/fleet/drivers', request)
    );
  }

  updateDriver(driverId: number, request: UpdateDriverRequest): Promise<DriverResponse> {
    return firstValueFrom(
      this.http.put<DriverResponse>(`/api/fleet/drivers/${driverId}`, request)
    );
  }

  deleteDriver(driverId: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/fleet/drivers/${driverId}`));
  }
}
