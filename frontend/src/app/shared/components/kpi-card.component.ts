import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [NgClass],
  template: `
    <article class="kpi-card" [ngClass]="variant()">
      <span class="kpi-label">{{ label() }}</span>
      <strong class="kpi-value">{{ value() }}</strong>
      @if (trend()) {
        <span class="kpi-trend">{{ trend() }}</span>
      }
      <span class="kpi-detail">{{ detail() }}</span>
    </article>
  `,
  styles: [`
    :host { display: contents; }

    .kpi-card {
      display: grid;
      grid-template-rows: auto 1fr auto auto;
      padding: 1.25rem 1.4rem 1.1rem;
      background: var(--surf);
      border: 1px solid var(--border);
      border-top: 3px solid var(--border-strong);
      border-radius: 2px;
    }

    .kpi-card.accent { border-top-color: var(--accent); }
    .kpi-card.alert  { border-top-color: var(--danger); }

    .kpi-label {
      display: block;
      font-family: var(--font-mono);
      font-size: 0.62rem;
      font-weight: 500;
      letter-spacing: 0.1em;
      text-transform: uppercase;
      color: var(--text-3);
      margin-bottom: 0.7rem;
    }

    .kpi-value {
      display: block;
      font-family: var(--font-mono);
      font-size: 2.4rem;
      font-weight: 400;
      line-height: 1;
      letter-spacing: -0.03em;
      color: var(--text-1);
      margin-bottom: 0.5rem;
    }

    .kpi-card.accent .kpi-value { color: var(--accent); }
    .kpi-card.alert  .kpi-value { color: var(--danger); }

    .kpi-trend {
      display: block;
      font-family: var(--font-mono);
      font-size: 0.68rem;
      color: var(--text-3);
      margin-bottom: 0.15rem;
    }

    .kpi-card.accent .kpi-trend { color: var(--success); }
    .kpi-card.alert  .kpi-trend { color: var(--danger); }

    .kpi-detail {
      display: block;
      font-size: 0.75rem;
      color: var(--text-3);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KpiCardComponent {
  readonly label   = input.required<string>();
  readonly value   = input.required<string>();
  readonly detail  = input.required<string>();
  readonly variant = input<'default' | 'accent' | 'alert'>('default');
  readonly trend   = input<string>('');
}
