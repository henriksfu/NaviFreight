export interface DashboardSummary {
  activeVehicles: number;
  yardOccupancyPercent: number;
  delayedLoads: number;
  onTimeDispatchRate: number;
  activeRoutes: number;
  trailerTurnaroundMinutes: number;
}

export interface FleetVehicle {
  vehicleId: string;
  driverName: string;
  status: 'In Transit' | 'At Dock' | 'Awaiting Dispatch' | 'Delayed';
  currentYard: string;
  lastUpdatedLabel: string;
  eta: string;
  utilizationPercent: number;
  routeCode: string;
}

export interface YardSnapshot {
  yardName: string;
  occupiedSlots: number;
  totalSlots: number;
  inboundQueue: number;
  availableDocks: number;
  averageTurnMinutes: number;
}

export interface TimelineItem {
  timeLabel: string;
  eventType: string;
  description: string;
}

export interface RouteAssignment {
  routeCode: string;
  origin: string;
  destination: string;
  status: 'On Schedule' | 'At Risk' | 'Delayed';
  assignedVehicles: number;
  nextDeparture: string;
  completionPercent: number;
}

export interface AlertItem {
  severity: 'Critical' | 'Warning' | 'Info';
  title: string;
  description: string;
  owner: string;
}

export interface ReportSnapshot {
  reportName: string;
  value: string;
  changeLabel: string;
}

export interface SettingsSection {
  title: string;
  items: string[];
}

export interface DashboardViewModel {
  tenantName: string;
  refreshLabel: string;
  summary: DashboardSummary;
  fleet: FleetVehicle[];
  yards: YardSnapshot[];
  timeline: TimelineItem[];
  routes: RouteAssignment[];
  alerts: AlertItem[];
  reports: ReportSnapshot[];
  settings: SettingsSection[];
}
