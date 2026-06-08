export type VehicleStatus = 'In Transit' | 'At Dock' | 'Awaiting Dispatch' | 'Delayed' | 'Available';
export type DriverAvailability = 'Available' | 'On Duty' | 'Off Duty' | 'On Leave';

export interface FleetVehicleResponse {
  vehicleId: string;
  driverName: string;
  status: string;
  currentYard: string;
  lastUpdatedUtc: string;
  etaUtc: string | null;
  utilizationPercent: number;
  routeCode: string;
}

export interface FleetVehicleDetailResponse {
  vehicleId: string;
  driverId: number | null;
  driverName: string;
  status: string;
  currentYardId: number | null;
  currentYard: string;
  lastUpdatedUtc: string;
  etaUtc: string | null;
  utilizationPercent: number;
  routeCode: string;
  isDispatchReady: boolean;
}

export interface DriverResponse {
  driverId: number;
  fullName: string;
  licenseNumber: string;
  availabilityStatus: string;
  assignedVehicleId: string | null;
}

export interface CreateVehicleRequest {
  vehicleId: string;
  status: string;
  currentYardId: number | null;
  etaUtc: string | null;
  utilizationPercent: number;
}

export interface UpdateVehicleRequest {
  status: string;
  currentYardId: number | null;
  etaUtc: string | null;
  utilizationPercent: number;
}

export interface CreateDriverRequest {
  fullName: string;
  licenseNumber: string;
  availabilityStatus: string;
}

export interface UpdateDriverRequest {
  fullName: string;
  licenseNumber: string;
  availabilityStatus: string;
}
