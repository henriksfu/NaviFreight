export type RouteStatus = 'On Schedule' | 'At Risk' | 'Delayed' | 'Completed';

export interface RouteAssignmentResponse {
  routeCode: string;
  origin: string;
  destination: string;
  status: string;
  assignedVehicles: number;
  nextDepartureUtc: string;
  completionPercent: number;
}

export interface RouteDispatchAssignmentResponse {
  vehicleId: string;
  driverName: string;
  status: string;
  currentYard: string;
  etaUtc: string | null;
}

export interface RouteDetailResponse {
  route: RouteAssignmentResponse;
  assignedVehicles: RouteDispatchAssignmentResponse[];
}

export interface CreateRouteRequest {
  routeCode: string;
  originYardId: number;
  destinationName: string;
  status: string;
  nextDepartureUtc: string;
  completionPercent: number;
}

export interface UpdateRouteRequest {
  destinationName: string;
  status: string;
  nextDepartureUtc: string;
  completionPercent: number;
}
