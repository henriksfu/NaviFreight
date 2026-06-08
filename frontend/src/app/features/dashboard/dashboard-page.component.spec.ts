import { TestBed } from '@angular/core/testing';
import { DashboardPageComponent } from './dashboard-page.component';

describe('DashboardPageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardPageComponent]
    }).compileComponents();
  });

  it('should render the dashboard title and tenant', () => {
    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h2')?.textContent).toContain('Fleet and yard command center');
    expect(compiled.textContent).toContain('Live Fleet Board');
    expect(compiled.textContent).toContain('VH-1042');
  });
});
