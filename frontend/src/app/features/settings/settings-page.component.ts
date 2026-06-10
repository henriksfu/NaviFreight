import {
  ChangeDetectionStrategy, Component, inject, OnInit, signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../core/services/toast.service';

interface TenantSetting { key: string; value: string; }

interface SettingField {
  key: string;
  label: string;
  value: string;
  placeholder?: string;
  type: 'text' | 'number';
}

const TENANT_KEYS = [
  { key: 'Brand profile',               label: 'Company Name',          type: 'text'   as const, placeholder: 'e.g. Atlas Meridian Logistics' },
  { key: 'Default operational region',  label: 'Operational Region',    type: 'text'   as const, placeholder: 'e.g. Pacific, Central, Northeast' },
  { key: 'Tenant header mapping',       label: 'Tenant Header',         type: 'text'   as const, placeholder: 'X-Tenant-Id' },
];

const RULE_KEYS = [
  { key: 'Auto-flag delays after',          label: 'Delay Flag Threshold',       type: 'text' as const, placeholder: 'e.g. 12 minutes' },
  { key: 'Recompute dock assignment every', label: 'Dock Recompute Interval',    type: 'text' as const, placeholder: 'e.g. 90 seconds' },
  { key: 'Escalate missed departure after', label: 'Escalation Retry Count',     type: 'text' as const, placeholder: 'e.g. 2 failed retries' },
];

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsPageComponent implements OnInit {
  private readonly http  = inject(HttpClient);
  private readonly toast = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly error     = signal<string | null>(null);
  protected readonly savingSection = signal<string | null>(null);

  protected tenantFields: SettingField[] = TENANT_KEYS.map(k => ({ ...k, value: '' }));
  protected ruleFields:   SettingField[] = RULE_KEYS.map(k => ({ ...k, value: '' }));

  async ngOnInit(): Promise<void> {
    try {
      const settings = await firstValueFrom(
        this.http.get<TenantSetting[]>('/api/settings/tenant')
      );
      const map = Object.fromEntries(settings.map(s => [s.key, s.value]));
      this.tenantFields = this.tenantFields.map(f => ({ ...f, value: map[f.key] ?? '' }));
      this.ruleFields   = this.ruleFields.map(f => ({ ...f, value: map[f.key] ?? '' }));
    } catch {
      this.error.set('Failed to load settings.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async saveSection(section: 'tenant' | 'rules'): Promise<void> {
    const fields = section === 'tenant' ? this.tenantFields : this.ruleFields;
    this.savingSection.set(section);
    try {
      await Promise.all(
        fields.map(f =>
          firstValueFrom(
            this.http.put<TenantSetting>(`/api/settings/tenant/${encodeURIComponent(f.key)}`, { value: f.value })
          )
        )
      );
      this.toast.success('Settings saved.');
    } catch {
      // error interceptor handles toast
    } finally {
      this.savingSection.set(null);
    }
  }
}
