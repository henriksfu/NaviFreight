import { NavigationItem } from '../models/navigation.models';

export const navigationItems: NavigationItem[] = [
  { label: 'Dashboard', route: '/', section: 'Operations' },
  { label: 'Fleet Board', route: '/fleet', section: 'Operations' },
  { label: 'Yards', route: '/yards', section: 'Operations' },
  { label: 'Routes', route: '/routes', section: 'Operations' },
  { label: 'Reports', route: '/reports', section: 'Analytics' },
  { label: 'Alerts', route: '/alerts', section: 'Analytics', badge: '3' },
  { label: 'Users', route: '/users', section: 'Admin' },
  { label: 'Settings', route: '/settings', section: 'Admin' }
];
