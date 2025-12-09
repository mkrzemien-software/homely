/**
 * Interface representing a household that a user belongs to
 */
export interface UserHousehold {
  householdId: string;
  householdName: string;
  role: 'admin' | 'member' | 'dashboard';
  joinedAt: Date;
  planTypeName: string;
}

/**
 * Request to add user to household
 */
export interface AddUserToHouseholdRequest {
  householdId: string;
  role: 'admin' | 'member' | 'dashboard';
}
