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
   * Unique identifier (UUID)
   */
  id: string;

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
   * Unique identifier (UUID)
   */
  id: string;

  /**
   * Parent category type ID (UUID)
   */
  categoryTypeId: string;

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
  categoryTypeId: string;
  categoryTypeName: string;
  categoryTypeDescription: string;
  categories: Category[];
  categoryCount: number;
}

/**
 * DTO for creating a new category
 */
export interface CreateCategoryDto {
  categoryTypeId: string;
  name: string;
  description: string;
  sortOrder?: number;
}

/**
 * DTO for updating an existing category
 */
export interface UpdateCategoryDto {
  categoryTypeId?: string;
  name?: string;
  isActive?: boolean;
  description?: string;
  sortOrder?: number;
}

/**
 * DTO for creating a new category type
 */
export interface CreateCategoryTypeDto {
  name: string;
  description: string;
  sortOrder?: number;
  isActive?: boolean;
}

/**
 * DTO for updating an existing category type
 */
export interface UpdateCategoryTypeDto {
  name?: string;
  description?: string;
  sortOrder?: number;
  isActive?: boolean;
}

/**
 * Single category sort order item
 */
export interface CategorySortOrderItem {
  id: string;
  sortOrder: number;
}

/**
 * DTO for updating multiple categories sort order
 */
export interface UpdateCategoriesSortOrderDto {
  items: CategorySortOrderItem[];
}

/**
 * Helper function to get category type color
 * Uses a hash of the UUID to determine color
 */
export function getCategoryTypeColor(categoryTypeId: string): string {
  const colors = ['primary', 'success', 'danger', 'warning', 'secondary', 'info', 'purple', 'teal', 'gray'];
  // Simple hash function for UUID to get consistent color
  let hash = 0;
  for (let i = 0; i < categoryTypeId.length; i++) {
    hash = categoryTypeId.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % colors.length;
  return colors[index];
}
