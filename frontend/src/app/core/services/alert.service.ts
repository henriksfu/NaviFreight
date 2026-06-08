import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  AlertResponse,
  CreateAlertRequest,
  AcknowledgeAlertRequest,
  ResolveAlertRequest,
  UpdateAlertOwnerRequest
} from '../models/alert.models';

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly http = inject(HttpClient);

  getAlerts(status?: string, severity?: string): Promise<{ tenantId: string; items: AlertResponse[] }> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    if (severity) params['severity'] = severity;
    return firstValueFrom(
      this.http.get<{ tenantId: string; items: AlertResponse[] }>('/api/alerts', { params })
    );
  }

  getAlert(alertId: number): Promise<AlertResponse> {
    return firstValueFrom(this.http.get<AlertResponse>(`/api/alerts/${alertId}`));
  }

  createAlert(request: CreateAlertRequest): Promise<AlertResponse> {
    return firstValueFrom(this.http.post<AlertResponse>('/api/alerts', request));
  }

  acknowledge(alertId: number, request: AcknowledgeAlertRequest): Promise<AlertResponse> {
    return firstValueFrom(
      this.http.post<AlertResponse>(`/api/alerts/${alertId}/acknowledge`, request)
    );
  }

  resolve(alertId: number, request: ResolveAlertRequest): Promise<AlertResponse> {
    return firstValueFrom(
      this.http.post<AlertResponse>(`/api/alerts/${alertId}/resolve`, request)
    );
  }

  reopen(alertId: number): Promise<AlertResponse> {
    return firstValueFrom(this.http.post<AlertResponse>(`/api/alerts/${alertId}/reopen`, {}));
  }

  updateOwner(alertId: number, request: UpdateAlertOwnerRequest): Promise<AlertResponse> {
    return firstValueFrom(
      this.http.put<AlertResponse>(`/api/alerts/${alertId}/owner`, request)
    );
  }

  close(alertId: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`/api/alerts/${alertId}`));
  }
}
