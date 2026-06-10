import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  AlertResponse,
  CreateAlertRequest,
  AcknowledgeAlertRequest,
  ResolveAlertRequest,
  UpdateAlertOwnerRequest
} from '../models/alert.models';
import { PagedResponse } from '../models/paged-response';

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly http = inject(HttpClient);

  getAlerts(status?: string, severity?: string, page = 1, pageSize = 20): Promise<PagedResponse<AlertResponse>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status)   params = params.set('status', status);
    if (severity) params = params.set('severity', severity);
    return firstValueFrom(
      this.http.get<PagedResponse<AlertResponse>>('/api/alerts', { params })
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
