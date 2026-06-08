import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

@Injectable({ providedIn: 'root' })
export class FleetService {
  private readonly http = inject(HttpClient);

  // ── Vehicles ──────────────────────────────────────────────────────────────

  getVehicles(): Promise<{ tenantId: string; items: FleetVehicleResponse[] }> {
    return firstValueFrom(
      this.http.get<{ tenantId: string; items: FleetVehicleResponse[] }>('/api/fleet/vehicles')
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

  getDrivers(): Promise<{ tenantId: string; items: DriverResponse[] }> {
    return firstValueFrom(
      this.http.get<{ tenantId: string; items: DriverResponse[] }>('/api/fleet/drivers')
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
