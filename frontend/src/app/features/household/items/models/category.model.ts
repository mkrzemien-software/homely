/**
 * Category Models
 *
 * Models for categories and category types based on API plan
 * See: .ai/api-plan.md sections:
 * - GET /category-types
 * - GET /categories
 */

/**
 * CategoryType interface
 * Represents high-level categories (technical inspections, waste collection, medical visits)
 */
export interface CategoryType {
  /**
   * Unique identifier
   */
  id: number;

  /**
   * Category type name
   * Example: "Przeglądy techniczne", "Wywóz odpadów", "Wizyty lekarskie"
   */
  name: string;

  /**
   * Category type description
   */
  description: string;

  /**
   * Display order
   */
  sortOrder: number;
}

/**
 * Category interface
 * Represents specific categories within types
 */
export interface Category {
  /**
   * Unique identifier
   */
  id: number;

  /**
   * Parent category type ID
   */
  categoryTypeId: number;

  /**
   * Category type name (returned from backend as flat field)
   */
  categoryTypeName?: string;

  /**
   * Category type details (populated from backend)
   * @deprecated Use categoryTypeName instead
   */
  categoryType?: CategoryType;

  /**
   * Category name
   * Example: "Kocioł gazowy", "Kompost", "Dentysta"
   */
  name: string;

  /**
   * Category description
   */
  description: string;

  /**
   * Display order within type
   */
  sortOrder: number;

  /**
   * Whether category is active
   */
  isActive?: boolean;

  /**
   * Creation timestamp
   */
  createdAt?: string;

  /**
   * Last update timestamp
   */
  updatedAt?: string;
}

/**
 * API Response for category types
 */
export interface CategoryTypesResponse {
  data: CategoryType[];
}

/**
 * API Response for categories
 */
export interface CategoriesResponse {
  data: Category[];
}

/**
 * Categories grouped by type
 */
export interface CategoriesByType {
  categoryTypeId: number;
  categoryTypeName: string;
  categoryTypeDescription: string;
  categories: Category[];
  categoryCount: number;
}

/**
 * DTO for creating a new category
 */
export interface CreateCategoryDto {
  categoryTypeId: number;
  name: string;
  description: string;
  sortOrder?: number;
}

/**
 * DTO for updating an existing category
 */
export interface UpdateCategoryDto {
  categoryTypeId?: number;
  name?: string;
  description?: string;
  sortOrder?: number;
}

/**
 * Helper function to get category type icon
 */
export function getCategoryTypeIcon(categoryTypeId: number): string {
  const iconMap: Record<number, string> = {
    1: 'pi-cog',           // Technical inspections
    2: 'pi-trash',         // Waste collection
    3: 'pi-heart-fill'     // Medical visits
  };
  return iconMap[categoryTypeId] || 'pi-tag';
}

/**
 * Helper function to get category type color
 */
export function getCategoryTypeColor(categoryTypeId: number): string {
  const colorMap: Record<number, string> = {
    1: 'primary',    // Technical inspections
    2: 'success',    // Waste collection
    3: 'danger'      // Medical visits
  };
  return colorMap[categoryTypeId] || 'secondary';
}
