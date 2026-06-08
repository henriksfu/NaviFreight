import { DashboardViewModel } from '../models/dashboard.models';

export const dashboardData: DashboardViewModel = {
  tenantName: 'Atlas Meridian Logistics',
  refreshLabel: 'Updated 90 seconds ago',
  summary: {
    activeVehicles: 184,
    yardOccupancyPercent: 72,
    delayedLoads: 14,
    onTimeDispatchRate: 96.4,
    activeRoutes: 28,
    trailerTurnaroundMinutes: 44
  },
  fleet: [
    {
      vehicleId: 'VH-1042',
      driverName: 'Ava Patel',
      status: 'In Transit',
      currentYard: 'Seattle North',
      lastUpdatedLabel: '8 min ago',
      eta: '11:20',
      utilizationPercent: 89,
      routeCode: 'NW-14'
    },
    {
      vehicleId: 'VH-1188',
      driverName: 'Marcus Gray',
      status: 'At Dock',
      currentYard: 'Portland East',
      lastUpdatedLabel: '15 min ago',
      eta: 'Docked',
      utilizationPercent: 74,
      routeCode: 'OR-07'
    },
    {
      vehicleId: 'VH-1211',
      driverName: 'Nina Chen',
      status: 'Awaiting Dispatch',
      currentYard: 'Seattle North',
      lastUpdatedLabel: '4 min ago',
      eta: '12:05',
      utilizationPercent: 62,
      routeCode: 'PN-22'
    },
    {
      vehicleId: 'VH-1305',
      driverName: 'Ethan Ross',
      status: 'Delayed',
      currentYard: 'Portland East',
      lastUpdatedLabel: '22 min ago',
      eta: '13:40',
      utilizationPercent: 51,
      routeCode: 'OR-03'
    },
    {
      vehicleId: 'VH-1417',
      driverName: 'Lena Brooks',
      status: 'In Transit',
      currentYard: 'Oakland Gateway',
      lastUpdatedLabel: '2 min ago',
      eta: '10:55',
      utilizationPercent: 93,
      routeCode: 'CA-11'
    }
  ],
  yards: [
    {
      yardName: 'Seattle North',
      occupiedSlots: 83,
      totalSlots: 120,
      inboundQueue: 6,
      availableDocks: 2,
      averageTurnMinutes: 38
    },
    {
      yardName: 'Portland East',
      occupiedSlots: 42,
      totalSlots: 75,
      inboundQueue: 3,
      availableDocks: 4,
      averageTurnMinutes: 46
    },
    {
      yardName: 'Oakland Gateway',
      occupiedSlots: 65,
      totalSlots: 90,
      inboundQueue: 8,
      availableDocks: 1,
      averageTurnMinutes: 49
    }
  ],
  timeline: [
    {
      timeLabel: '09:10',
      eventType: 'Dock Release',
      description: 'Dock 4 reopened after unload completion.'
    },
    {
      timeLabel: '08:56',
      eventType: 'Route Alert',
      description: 'Northbound route congestion pushed ETA by 14 minutes.'
    },
    {
      timeLabel: '08:42',
      eventType: 'Fleet Sync',
      description: 'Global telematics heartbeat reconciled 182 active assets.'
    }
  ],
  routes: [
    {
      routeCode: 'NW-14',
      origin: 'Seattle North',
      destination: 'Spokane Hub',
      status: 'On Schedule',
      assignedVehicles: 5,
      nextDeparture: '10:30',
      completionPercent: 68
    },
    {
      routeCode: 'OR-03',
      origin: 'Portland East',
      destination: 'Boise Crossdock',
      status: 'Delayed',
      assignedVehicles: 3,
      nextDeparture: '11:10',
      completionPercent: 42
    },
    {
      routeCode: 'CA-11',
      origin: 'Oakland Gateway',
      destination: 'Reno Freight Park',
      status: 'At Risk',
      assignedVehicles: 4,
      nextDeparture: '12:15',
      completionPercent: 57
    }
  ],
  alerts: [
    {
      severity: 'Critical',
      title: 'Trailer queue exceeding target',
      description: 'Oakland Gateway has remained above 7 inbound trailers for 35 minutes.',
      owner: 'Yard Ops'
    },
    {
      severity: 'Warning',
      title: 'Dispatch SLA drift',
      description: 'Northwest outbound wave is 9 minutes behind target dispatch time.',
      owner: 'Routing'
    },
    {
      severity: 'Info',
      title: 'Fleet sync completed',
      description: 'Global telematics reconciliation completed without dropped assets.',
      owner: 'Platform'
    }
  ],
  reports: [
    {
      reportName: 'On-time departure',
      value: '96.4%',
      changeLabel: '+1.2% week over week'
    },
    {
      reportName: 'Average dock turn',
      value: '44 min',
      changeLabel: '-3 min versus last week'
    },
    {
      reportName: 'Asset utilization',
      value: '81%',
      changeLabel: '+4 points this month'
    }
  ],
  settings: [
    {
      title: 'Tenant Controls',
      items: [
        'Tenant header mapping: X-Tenant-Id',
        'Default operational region: Pacific',
        'Brand profile: Atlas Meridian Logistics'
      ]
    },
    {
      title: 'Dispatch Rules',
      items: [
        'Auto-flag delays after 12 minutes',
        'Recompute dock assignment every 90 seconds',
        'Escalate missed departure after 2 failed retries'
      ]
    }
  ]
};
