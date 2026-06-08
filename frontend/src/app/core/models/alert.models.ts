export type AlertSeverity = 'Critical' | 'Warning' | 'Info';
export type AlertStatus = 'Active' | 'Acknowledged' | 'Resolved' | 'Closed';

export interface AlertResponse {
  alertEventId: number;
  severity: AlertSeverity;
  title: string;
  description: string;
  ownerTeam: string;
  status: AlertStatus;
  isActive: boolean;
  createdUtc: string;
  acknowledgedByEmail: string | null;
  acknowledgedUtc: string | null;
  resolvedByEmail: string | null;
  resolvedUtc: string | null;
  resolutionNotes: string | null;
  updatedUtc: string | null;
}

export interface CreateAlertRequest {
  severity: string;
  title: string;
  description: string;
  ownerTeam: string;
}

export interface AcknowledgeAlertRequest {
  acknowledgedByEmail: string;
}

export interface ResolveAlertRequest {
  resolvedByEmail: string;
  resolutionNotes: string | null;
}

export interface UpdateAlertOwnerRequest {
  ownerTeam: string;
}
