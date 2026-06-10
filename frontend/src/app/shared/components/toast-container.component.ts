import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [NgClass],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast-stack" aria-live="polite" aria-atomic="false">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [ngClass]="'toast--' + toast.type" role="alert">
          <span class="toast-icon" aria-hidden="true">
            @switch (toast.type) {
              @case ('success') { ✓ }
              @case ('error')   { ✕ }
              @case ('warn')    { ⚠ }
              @default          { ℹ }
            }
          </span>
          <span class="toast-msg">{{ toast.message }}</span>
          <button class="toast-close" type="button" (click)="toastService.dismiss(toast.id)" aria-label="Dismiss">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-stack {
      position: fixed;
      bottom: 1.5rem;
      right: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      z-index: 9999;
      max-width: 22rem;
      pointer-events: none;
    }
    .toast {
      display: flex;
      align-items: flex-start;
      gap: 0.6rem;
      padding: 0.75rem 1rem;
      border-radius: 3px;
      font-size: 0.875rem;
      line-height: 1.4;
      border: 1px solid transparent;
      pointer-events: all;
      animation: slide-in 160ms ease;
      background: var(--surf-raised, #1e1e1e);
      color: var(--text-1, #f5f5f5);
      box-shadow: 0 4px 16px rgba(0,0,0,.35);
    }
    @keyframes slide-in {
      from { transform: translateX(110%); opacity: 0; }
      to   { transform: translateX(0);    opacity: 1; }
    }
    .toast--success { border-color: var(--success, #10b981); }
    .toast--error   { border-color: var(--danger,  #ef4444); }
    .toast--warn    { border-color: var(--warn,    #f59e0b); }
    .toast--info    { border-color: var(--accent,  #6366f1); }
    .toast-icon {
      flex-shrink: 0;
      font-style: normal;
      font-weight: 700;
      font-size: 0.8rem;
      margin-top: 0.05rem;
    }
    .toast--success .toast-icon { color: var(--success, #10b981); }
    .toast--error   .toast-icon { color: var(--danger,  #ef4444); }
    .toast--warn    .toast-icon { color: var(--warn,    #f59e0b); }
    .toast--info    .toast-icon { color: var(--accent,  #6366f1); }
    .toast-msg  { flex: 1; }
    .toast-close {
      flex-shrink: 0;
      background: none;
      border: none;
      color: var(--text-3, #888);
      cursor: pointer;
      font-size: 1.1rem;
      line-height: 1;
      padding: 0;
      margin-top: -0.1rem;
    }
    .toast-close:hover { color: var(--text-1, #f5f5f5); }
  `]
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
