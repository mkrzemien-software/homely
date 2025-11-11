import { NavigationTile } from '../../../system/dashboard/models/navigation-tile.model';

/**
 * Default navigation tiles for Household Dashboard
 *
 * Based on PRD section 3.4.1:
 * Dashboard główny powinien zawierać kafelki nawigacyjne (duże przyciski z ikonami)
 * do przełączania widoków:
 * - 📋 Zadania - lista nadchodzących terminów (7 dni) z szybkimi akcjami
 * - 🏷️ Kategorie - widok urządzeń/wizyt pogrupowanych po kategoriach
 * - ⚙️ Ustawienia - szybki dostęp do konfiguracji gospodarstwa
 *
 * @param householdId - The ID of the household
 * @returns Array of navigation tiles configured for the household
 */
export function getHouseholdNavigationTiles(householdId: string): NavigationTile[] {
  return [
    {
      id: 'tasks',
      title: 'Zadania',
      description: 'Lista nadchodzących terminów z szybkimi akcjami',
      icon: 'pi pi-list-check',
      route: `/${householdId}/tasks`,
      color: 'primary',
      enabled: true,
      stats: {
        label: 'Nadchodzące w 7 dni',
        value: 0,
        trend: 'neutral'
      }
    },
    {
      id: 'categories',
      title: 'Kategorie',
      description: 'Urządzenia i wizyty pogrupowane po kategoriach',
      icon: 'pi pi-tags',
      route: `/${householdId}/categories`,
      color: 'success',
      enabled: true,
      stats: {
        label: 'Aktywnych kategorii',
        value: 0
      }
    },
    {
      id: 'settings',
      title: 'Ustawienia',
      description: 'Konfiguracja gospodarstwa i zarządzanie członkami',
      icon: 'pi pi-cog',
      route: `/${householdId}/settings`,
      color: 'secondary',
      enabled: true
    }
  ];
}

/**
 * Navigation tiles specifically for Administrator role
 * Includes all tiles with full access
 */
export function getAdminNavigationTiles(householdId: string): NavigationTile[] {
  return getHouseholdNavigationTiles(householdId);
}

/**
 * Navigation tiles for Member (Domownik) role
 * Limited access - can view tasks and categories, but limited settings access
 */
export function getMemberNavigationTiles(householdId: string): NavigationTile[] {
  const tiles = getHouseholdNavigationTiles(householdId);

  // Modify settings tile for members
  return tiles.map(tile => {
    if (tile.id === 'settings') {
      return {
        ...tile,
        title: 'Mój profil',
        description: 'Ustawienia profilu i preferencje powiadomień',
        route: `/${householdId}/profile`
      };
    }
    return tile;
  });
}

/**
 * Navigation tiles for Dashboard role (read-only)
 * Only view tasks - optimized for wall-mounted displays
 */
export function getDashboardRoleNavigationTiles(householdId: string): NavigationTile[] {
  const tiles = getHouseholdNavigationTiles(householdId);

  // Dashboard role only sees tasks
  return tiles.filter(tile => tile.id === 'tasks');
}
