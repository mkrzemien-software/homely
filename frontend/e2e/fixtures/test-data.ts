/**
 * Test Data Fixtures
 * Centralized test data for E2E tests
 */

/**
 * Generate unique name with timestamp suffix
 * Ensures that test data doesn't conflict when running multiple times
 */
export function generateUniqueName(baseName: string): string {
  const timestamp = Date.now();
  return `${baseName} ${timestamp}`;
}

export const TEST_USERS = {
  admin: {
    email: 'admin@e2e.homely.com',
    password: 'Test123!@#',
  },
  member: {
    email: 'member@e2e.homely.com',
    password: 'Test123!@#',
  },
  dashboard: {
    email: 'dashboard@e2e.homely.com',
    password: 'Test123!@#',
  },
};

export const TEST_CATEGORY_TYPES = {
  technicalInspections: {
    name: 'Technical Inspections',
    description: 'Vehicle and home technical inspections',
  },
  wasteCollection: {
    name: 'Waste Collection',
    description: 'Garbage and recycling collection schedules',
  },
  medicalVisits: {
    name: 'Medical Visits',
    description: 'Doctor appointments and health checkups',
  },
};

export const TEST_CATEGORIES = {
  carInspection: {
    name: 'Car Inspection',
    description: 'Annual car safety and emissions inspection',
    categoryType: 'Technical Inspections',
  },
  homeInspection: {
    name: 'Home Inspection',
    description: 'Home safety and maintenance inspection',
    categoryType: 'Technical Inspections',
  },
  recyclableWaste: {
    name: 'Recyclable Waste',
    description: 'Plastic, paper, and glass recycling',
    categoryType: 'Waste Collection',
  },
  organicWaste: {
    name: 'Organic Waste',
    description: 'Compostable and food waste',
    categoryType: 'Waste Collection',
  },
  dentalCheckup: {
    name: 'Dental Checkup',
    description: 'Regular dental examination',
    categoryType: 'Medical Visits',
  },
};
