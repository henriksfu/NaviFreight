export interface YardResponse {
  yardId: number;
  yardName: string;
  capacity: number;
  occupiedSlots: number;
  inboundQueue: number;
  averageTurnMinutes: number;
  totalDocks: number;
  availableDocks: number;
  isActive: boolean;
  createdUtc: string;
  updatedUtc: string | null;
}

export interface DockResponse {
  dockId: number;
  yardId: number;
  dockCode: string;
  status: 'Available' | 'Occupied' | 'Maintenance' | 'Offline';
  occupyingVehicleId: string | null;
  notes: string | null;
  createdUtc: string;
  updatedUtc: string | null;
}

export interface YardDetailResponse {
  yard: YardResponse;
  docks: DockResponse[];
}

export interface CreateYardRequest {
  yardName: string;
  capacity: number;
}

export interface UpdateYardRequest {
  yardName: string;
  capacity: number;
}

export interface YardOperationalUpdateRequest {
  occupiedSlots: number;
  inboundQueue: number;
  averageTurnMinutes: number;
}

export interface CreateDockRequest {
  dockCode: string;
  status: string;
}

export interface UpdateDockRequest {
  dockCode: string;
  status: string;
  notes: string | null;
}
