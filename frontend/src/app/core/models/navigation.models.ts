export interface NavigationItem {
  label: string;
  route: string;
  section: 'Operations' | 'Analytics' | 'Admin';
  badge?: string;
}
