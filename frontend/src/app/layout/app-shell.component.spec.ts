import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from '../app.routes';
import { AppShellComponent } from './app-shell.component';

describe('AppShellComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShellComponent],
      providers: [
        provideRouter(routes),
        provideHttpClient()
      ]
    }).compileComponents();
  });

  it('should render primary product heading and navigation', () => {
    const fixture = TestBed.createComponent(AppShellComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')?.textContent).toContain('NaviFreight');
    expect(compiled.textContent).toContain('Fleet Board');
    expect(compiled.textContent).toContain('Alerts');
  });
});
