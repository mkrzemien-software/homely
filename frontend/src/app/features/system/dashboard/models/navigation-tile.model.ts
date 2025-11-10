/**
 * Navigation Tile Model
 *
 * Represents a navigation tile in the System Dashboard.
 * Each tile links to a main section of the system administration.
 */
export interface NavigationTile {
  /**
   * Unique identifier for the tile
   */
  id: string;

  /**
   * Display title of the tile
   */
  title: string;

  /**
   * Brief description of the section
   */
  description: string;

  /**
   * PrimeIcons icon class (e.g., 'pi pi-users')
   */
  icon: string;

  /**
   * Router path to navigate to
   */
  route: string;

  /**
   * Color theme for the tile
   * Used for icon and hover effects
   */
  color: TileColor;

  /**
   * Optional badge count for alerts/notifications
   */
  badgeCount?: number;

  /**
   * Optional badge severity for styling
   * Note: PrimeNG Badge uses 'warn' instead of 'warning'
   */
  badgeSeverity?: 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

  /**
   * Optional statistics to display on the tile
   */
  stats?: TileStats;

  /**
   * Whether the tile is enabled/accessible
   */
  enabled?: boolean;
}

/**
 * Color theme options for tiles
 */
export type TileColor =
  | 'primary'   // Blue - default
  | 'success'   // Green
  | 'info'      // Light blue
  | 'warning'   // Orange
  | 'danger'    // Red
  | 'secondary' // Gray
  | 'purple'    // Purple
  | 'teal';     // Teal

/**
 * Statistics displayed on a tile
 */
export interface TileStats {
  /**
   * Label for the statistic
   */
  label: string;

  /**
   * Value to display
   */
  value: string | number;

  /**
   * Optional trend indicator
   */
  trend?: 'up' | 'down' | 'neutral';

  /**
   * Optional trend percentage
   */
  trendValue?: number;
}

/**
 * Default navigation tiles for System Dashboard
 */
export const DEFAULT_NAVIGATION_TILES: NavigationTile[] = [
  {
    id: 'households',
    title: 'Gospodarstwa',
    description: 'Zarządzanie wszystkimi gospodarstwami w systemie',
    icon: 'pi pi-home',
    route: '/system/households',
    color: 'primary',
    enabled: true
  },
  {
    id: 'users',
    title: 'Użytkownicy',
    description: 'Administracja kontami użytkowników globalnie',
    icon: 'pi pi-users',
    route: '/system/users',
    color: 'success',
    enabled: true
  },
  {
    id: 'subscriptions',
    title: 'Subskrypcje',
    description: 'Monitorowanie subskrypcji i metryk finansowych',
    icon: 'pi pi-credit-card',
    route: '/system/subscriptions',
    color: 'warning',
    enabled: false, // Post-MVP
    badgeCount: 0
  },
  {
    id: 'administration',
    title: 'Administracja',
    description: 'Monitoring techniczny i zarządzanie infrastrukturą',
    icon: 'pi pi-cog',
    route: '/system/administration',
    color: 'danger',
    enabled: false // Post-MVP
  },
  {
    id: 'support',
    title: 'Wsparcie',
    description: 'Narzędzia do udzielania wsparcia użytkownikom',
    icon: 'pi pi-headphones',
    route: '/system/support',
    color: 'info',
    enabled: false // Post-MVP
  }
];
