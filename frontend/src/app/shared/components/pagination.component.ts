import {
  ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output
} from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (totalPages > 1) {
      <nav class="pagination">
        <button class="pg-btn" [disabled]="currentPage <= 1" (click)="go(currentPage - 1)">‹</button>

        @for (p of pages; track p) {
          @if (p === 0) {
            <span class="pg-ellipsis">…</span>
          } @else {
            <button class="pg-btn" [class.active]="p === currentPage" (click)="go(p)">{{ p }}</button>
          }
        }

        <button class="pg-btn" [disabled]="currentPage >= totalPages" (click)="go(currentPage + 1)">›</button>
      </nav>
    }
  `,
  styles: [`
    .pagination {
      display: flex;
      align-items: center;
      gap: 0.2rem;
      padding: 0.75rem 0 0;
      justify-content: flex-end;
    }
    .pg-btn {
      min-width: 2rem;
      height: 2rem;
      padding: 0 0.4rem;
      background: transparent;
      border: 1px solid var(--border);
      border-radius: 2px;
      color: var(--text-2);
      font-size: 0.8rem;
      font-family: var(--font-mono);
      cursor: pointer;
      transition: background 60ms;
      &:hover:not(:disabled):not(.active) { background: var(--surf-raised); }
      &.active { background: var(--accent); color: #fff; border-color: var(--accent); }
      &:disabled { opacity: 0.35; cursor: not-allowed; }
    }
    .pg-ellipsis { color: var(--text-3); font-size: 0.8rem; padding: 0 0.2rem; }
  `]
})
export class PaginationComponent implements OnChanges {
  @Input() currentPage = 1;
  @Input() totalPages  = 1;
  @Output() pageChange = new EventEmitter<number>();

  pages: number[] = [];

  ngOnChanges(): void {
    this.pages = this.buildPages();
  }

  go(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.pageChange.emit(page);
    }
  }

  private buildPages(): number[] {
    const total = this.totalPages;
    const cur   = this.currentPage;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

    const pages: number[] = [1];
    if (cur > 3) pages.push(0);
    const start = Math.max(2, cur - 1);
    const end   = Math.min(total - 1, cur + 1);
    for (let i = start; i <= end; i++) pages.push(i);
    if (cur < total - 2) pages.push(0);
    pages.push(total);
    return pages;
  }
}
