import { NavigationTile } from '../../../system/dashboard/models/navigation-tile.model';

/**
 * Default navigation tiles for Household Dashboard
 *
 * Based on PRD section 3.4.1 and UI Plan (ui-plan.md line 216-221):
 * Dashboard główny powinien zawierać 4 kafelki nawigacyjne (duże przyciski z ikonami)
 * w układzie 2x2 (desktop) / pionowym (mobile):
 * - 📅 Kalendarz → `/calendar` - widok miesięczny kalendarz
 * - 📋 Wydarzenia → `/events` - lista wszystkich wydarzeń
 * - 📝 Zadania → `/tasks` - zarządzanie szablonami zadań
 * - 🏷️ Kategorie → `/categories` - widok kategorii i podkategorii z zadaniami
 *
 * @param householdId - The ID of the household
 * @returns Array of navigation tiles configured for the household (2x2 layout)
 */
export function getHouseholdNavigationTiles(householdId: string): NavigationTile[] {
  return [
    {
      id: 'calendar',
      title: 'Kalendarz',
      description: 'Widok miesięczny z zaznaczonymi wydarzeniami',
      icon: 'pi pi-calendar',
      route: `/${householdId}/calendar`,
      color: 'primary',
      enabled: true,
      stats: {
        label: 'Wydarzeń w tym miesiącu',
        value: 0,
        trend: 'neutral'
      }
    },
    {
      id: 'events',
      title: 'Wydarzenia',
      description: 'Lista wszystkich wydarzeń z filtrowaniem',
      icon: 'pi pi-calendar-plus',
      route: `/${householdId}/events`,
      color: 'info',
      enabled: true,
      stats: {
        label: 'Nadchodzących wydarzeń',
        value: 0,
        trend: 'neutral'
      }
    },
    {
      id: 'tasks',
      title: 'Zadania',
      description: 'Zarządzanie szablonami zadań',
      icon: 'pi pi-list-check',
      route: `/${householdId}/tasks`,
      color: 'success',
      enabled: true,
      stats: {
        label: 'Aktywnych zadań',
        value: 0,
        trend: 'neutral'
      }
    },
    {
      id: 'categories',
      title: 'Kategorie',
      description: 'Widok kategorii i podkategorii z zadaniami',
      icon: 'pi pi-tags',
      route: `/${householdId}/categories`,
      color: 'warning',
      enabled: true,
      stats: {
        label: 'Kategorii',
        value: 0
      }
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
 * Full access to all 4 navigation tiles
 */
export function getMemberNavigationTiles(householdId: string): NavigationTile[] {
  // Members have access to all tiles
  return getHouseholdNavigationTiles(householdId);
}

/**
 * Navigation tiles for Dashboard role (read-only)
 * Limited to viewing Calendar and Events only - optimized for wall-mounted displays
 */
export function getDashboardRoleNavigationTiles(householdId: string): NavigationTile[] {
  const tiles = getHouseholdNavigationTiles(householdId);

  // Dashboard role only sees Calendar and Events (read-only views)
  return tiles.filter(tile => tile.id === 'calendar' || tile.id === 'events');
}
