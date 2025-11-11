/**
 * Role types for user permissions
 */
export type UserRole = 'admin' | 'member' | 'dashboard' | 'system_developer';

/**
 * Menu item interface for sidebar navigation
 */
export interface MenuItem {
  id: string;
  label: string;
  icon: string;
  route?: string;
  roles: UserRole[];
  requiresPremium?: boolean;
  badge?: string;
  tooltip?: string;
  children?: MenuItem[];
}

/**
 * Menu section interface
 */
export interface MenuSection {
  id: string;
  title: string;
  items: MenuItem[];
  roles: UserRole[];
  showSeparator?: boolean;
}

/**
 * Get household menu items (Section 1)
 */
export function getHouseholdMenuItems(householdId: string): MenuItem[] {
  return [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: 'pi pi-home',
      route: `/${householdId}/dashboard`,
      roles: ['admin', 'member', 'dashboard'],
      tooltip: 'Główny widok z kafelkami i kalendarzem'
    },
    {
      id: 'tasks',
      label: 'Zadania',
      icon: 'pi pi-list-check',
      route: `/${householdId}/tasks`,
      roles: ['admin', 'member'],
      tooltip: 'Lista nadchodzących terminów (7 dni)'
    },
    {
      id: 'categories',
      label: 'Kategorie',
      icon: 'pi pi-tags',
      route: `/${householdId}/categories`,
      roles: ['admin', 'member'],
      tooltip: 'Urządzenia i wizyty pogrupowane po kategoriach'
    },
    {
      id: 'items',
      label: 'Urządzenia/Wizyty',
      icon: 'pi pi-box',
      route: `/${householdId}/items`,
      roles: ['admin', 'member'],
      tooltip: 'Pełna lista z możliwością zarządzania'
    },
    {
      id: 'household-manage',
      label: 'Gospodarstwo',
      icon: 'pi pi-users',
      route: `/${householdId}/manage`,
      roles: ['admin'],
      tooltip: 'Zarządzanie członkami i ustawieniami'
    },
    {
      id: 'history',
      label: 'Historia',
      icon: 'pi pi-history',
      route: `/${householdId}/history`,
      roles: ['admin', 'member'],
      requiresPremium: true,
      badge: 'PREMIUM',
      tooltip: 'Archiwum wykonanych zadań'
    },
    {
      id: 'reports',
      label: 'Raporty',
      icon: 'pi pi-chart-bar',
      route: `/${householdId}/reports`,
      roles: ['admin', 'member'],
      requiresPremium: true,
      badge: 'PREMIUM',
      tooltip: 'Zestawienia kosztów'
    },
    {
      id: 'analytics',
      label: 'Analizy',
      icon: 'pi pi-chart-line',
      route: `/${householdId}/analytics`,
      roles: ['admin', 'member'],
      requiresPremium: true,
      badge: 'PREMIUM',
      tooltip: 'Zaawansowane analizy predykcyjne'
    },
    {
      id: 'settings',
      label: 'Ustawienia',
      icon: 'pi pi-cog',
      route: `/${householdId}/settings`,
      roles: ['admin', 'member'],
      tooltip: 'Konfiguracja profilu i preferencji'
    },
    {
      id: 'help',
      label: 'Pomoc',
      icon: 'pi pi-question-circle',
      route: `/${householdId}/help`,
      roles: ['admin', 'member', 'dashboard'],
      tooltip: 'FAQ i wsparcie'
    }
  ];
}

/**
 * Get system menu items (Section 2 - System Developer only)
 */
export function getSystemMenuItems(): MenuItem[] {
  return [
    {
      id: 'system-dashboard',
      label: 'System Dashboard',
      icon: 'pi pi-desktop',
      route: '/system/dashboard',
      roles: ['system_developer'],
      tooltip: 'Główny panel administracyjny platformy'
    },
    {
      id: 'system-households',
      label: 'Gospodarstwa',
      icon: 'pi pi-building',
      route: '/system/households',
      roles: ['system_developer'],
      tooltip: 'Zarządzanie wszystkimi gospodarstwami w systemie'
    },
    {
      id: 'system-users',
      label: 'Użytkownicy',
      icon: 'pi pi-user',
      route: '/system/users',
      roles: ['system_developer'],
      tooltip: 'Administracja wszystkich kont użytkowników'
    },
    {
      id: 'system-subscriptions',
      label: 'Subskrypcje',
      icon: 'pi pi-credit-card',
      route: '/system/subscriptions',
      roles: ['system_developer'],
      tooltip: 'Monitoring płatności i metryk finansowych'
    },
    {
      id: 'system-administration',
      label: 'Administracja',
      icon: 'pi pi-wrench',
      route: '/system/administration',
      roles: ['system_developer'],
      tooltip: 'Zarządzanie infrastrukturą i konfiguracją'
    },
    {
      id: 'system-support',
      label: 'Wsparcie',
      icon: 'pi pi-headphones',
      route: '/system/support',
      roles: ['system_developer'],
      tooltip: 'Narzędzia do obsługi użytkowników i troubleshooting'
    },
    {
      id: 'system-metrics',
      label: 'Metryki Systemu',
      icon: 'pi pi-chart-pie',
      route: '/system/metrics',
      roles: ['system_developer'],
      tooltip: 'Globalne statystyki i KPI platformy'
    },
    {
      id: 'system-configuration',
      label: 'Konfiguracja Systemu',
      icon: 'pi pi-sliders-h',
      route: '/system/configuration',
      roles: ['system_developer'],
      tooltip: 'Ustawienia globalne platformy'
    }
  ];
}

/**
 * Get all menu sections
 */
export function getMenuSections(
  householdId: string | null,
  userRole: UserRole,
  hasPremium: boolean
): MenuSection[] {
  const sections: MenuSection[] = [];

  // Section 1: Household Views
  if (householdId && ['admin', 'member', 'dashboard'].includes(userRole)) {
    const householdItems = getHouseholdMenuItems(householdId);
    const filteredItems = householdItems.filter(item => {
      // Filter by role
      if (!item.roles.includes(userRole)) {
        return false;
      }
      // Filter premium items if user doesn't have premium
      if (item.requiresPremium && !hasPremium) {
        return false;
      }
      return true;
    });

    sections.push({
      id: 'household-section',
      title: 'GOSPODARSTWO',
      items: filteredItems,
      roles: ['admin', 'member', 'dashboard'],
      showSeparator: true
    });
  }

  // Section 2: System Views (only for System Developer)
  if (userRole === 'system_developer') {
    sections.push({
      id: 'system-section',
      title: 'ADMINISTRACJA SYSTEMU',
      items: getSystemMenuItems(),
      roles: ['system_developer'],
      showSeparator: false
    });
  }

  return sections;
}

/**
 * Check if user has access to a menu item
 */
export function hasMenuAccess(
  item: MenuItem,
  userRole: UserRole,
  hasPremium: boolean
): boolean {
  if (!item.roles.includes(userRole)) {
    return false;
  }
  if (item.requiresPremium && !hasPremium) {
    return false;
  }
  return true;
}
