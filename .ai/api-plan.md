# REST API Plan

## 1. Resources

- **Profiles** - User profile data (`user_profiles` table)
- **Households** - Household management (`households` table)
- **HouseholdMembers** - Household membership with roles (`household_members` table)
- **PlanTypes** - Subscription plan definitions (`plan_types` table)
- **CategoryTypes** - High-level categories (`category_types` table)
- **Categories** - Specific categories within types (`categories` table)
- **Items** - Devices and visits (`items` table)
- **Tasks** - Scheduled appointments (`tasks` table)
- **TasksHistory** - Completed tasks archive (`tasks_history` table)
- **Dashboard** - Aggregated dashboard data (multiple tables via views)

## 2. Endpoints

### Authentication

#### POST /auth/register
- **Description**: Register new user account
- **Request Body**:
```json
{
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string"
}
```
- **Response**: 
  - **201**: `{ "message": "Registration successful", "userId": "uuid" }`
  - **400**: `{ "error": "Validation failed", "details": ["email already exists"] }`
  - **422**: `{ "error": "Invalid input format" }`

#### POST /auth/login
- **Description**: Authenticate user
- **Request Body**:
```json
{
  "email": "string",
  "password": "string",
  "rememberMe": "boolean"
}
```
- **Response**: 
  - **200**: `{ "token": "jwt_token", "user": { "id": "uuid", "email": "string" } }`
  - **401**: `{ "error": "Invalid credentials" }`

#### POST /auth/logout
- **Description**: Logout current user
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Logout successful" }`

#### POST /auth/reset-password
- **Description**: Initiate password reset
- **Request Body**:
```json
{
  "email": "string"
}
```
- **Response**: 
  - **200**: `{ "message": "Reset email sent" }`

### Profiles

#### GET /profiles/me
- **Description**: Get current user profile
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "id": "uuid", "firstName": "string", "lastName": "string", "avatarUrl": "string", "lastActiveAt": "datetime" }`
  - **401**: `{ "error": "Unauthorized" }`

#### PUT /profiles/me
- **Description**: Update current user profile
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "firstName": "string",
  "lastName": "string",
  "avatarUrl": "string"
}
```
- **Response**: 
  - **200**: Updated profile object
  - **400**: `{ "error": "Validation failed", "details": [] }`

### Plan Types

#### GET /plan-types
- **Description**: Get available subscription plans
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "integer",
      "name": "string",
      "description": "string",
      "maxHouseholdMembers": "integer",
      "maxItems": "integer",
      "priceMonthly": "decimal",
      "priceYearly": "decimal",
      "features": ["string"]
    }
  ]
}
```

### Households

#### GET /households
- **Description**: Get households for current user
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "uuid",
      "name": "string",
      "address": "string",
      "planType": {
        "id": "integer",
        "name": "string",
        "maxHouseholdMembers": "integer",
        "maxItems": "integer"
      },
      "subscriptionStatus": "string",
      "userRole": "string",
      "createdAt": "datetime"
    }
  ]
}
```

#### POST /households
- **Description**: Create new household
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "name": "string",
  "address": "string"
}
```
- **Response**: 
  - **201**: Created household object
  - **400**: `{ "error": "Validation failed" }`

#### GET /households/{id}
- **Description**: Get specific household details
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: Household object with member count and usage statistics
  - **404**: `{ "error": "Household not found" }`
  - **403**: `{ "error": "Access denied" }`

#### PUT /households/{id}
- **Description**: Update household (admin only)
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "name": "string",
  "address": "string"
}
```
- **Response**: 
  - **200**: Updated household object
  - **403**: `{ "error": "Admin access required" }`

#### DELETE /households/{id}
- **Description**: Soft delete household (admin only)
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Household deleted" }`
  - **403**: `{ "error": "Admin access required" }`

### Household Members

#### GET /households/{householdId}/members
- **Description**: Get household members
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "uuid",
      "user": {
        "id": "uuid",
        "firstName": "string",
        "lastName": "string",
        "email": "string"
      },
      "role": "string",
      "joinedAt": "datetime"
    }
  ]
}
```

#### POST /households/{householdId}/members
- **Description**: Invite new member (admin only)
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "email": "string",
  "role": "string"
}
```
- **Response**: 
  - **201**: `{ "message": "Invitation sent", "invitationId": "uuid" }`
  - **400**: `{ "error": "Member limit reached for free plan" }`
  - **403**: `{ "error": "Admin access required" }`

#### PUT /households/{householdId}/members/{memberId}
- **Description**: Update member role (admin only)
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "role": "string"
}
```
- **Response**: 
  - **200**: Updated member object
  - **403**: `{ "error": "Admin access required" }`

#### DELETE /households/{householdId}/members/{memberId}
- **Description**: Remove member (admin only)
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Member removed" }`
  - **403**: `{ "error": "Admin access required" }`

### Category Types

#### GET /category-types
- **Description**: Get available category types
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "integer",
      "name": "string",
      "description": "string",
      "sortOrder": "integer"
    }
  ]
}
```

### Categories

#### GET /categories
- **Description**: Get categories by type
- **Query Parameters**: `categoryTypeId` (optional)
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "integer",
      "categoryTypeId": "integer",
      "name": "string",
      "description": "string",
      "sortOrder": "integer"
    }
  ]
}
```

### Items

#### GET /items
- **Description**: Get items for user's households
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `categoryId` (optional)
  - `assignedTo` (optional)
  - `priority` (optional)
  - `isActive` (optional, default: true)
  - `sortBy` (optional: name, priority, nextDueDate)
  - `sortOrder` (optional: asc, desc)
  - `page` (optional, default: 1)
  - `limit` (optional, default: 20, max: 100)
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "uuid",
      "householdId": "uuid",
      "category": {
        "id": "integer",
        "name": "string",
        "categoryType": {
          "id": "integer",
          "name": "string"
        }
      },
      "name": "string",
      "description": "string",
      "interval": {
        "years": "integer",
        "months": "integer",
        "weeks": "integer",
        "days": "integer"
      },
      "lastDate": "date",
      "nextDueDate": "date",
      "priority": "string",
      "notes": "string",
      "isActive": "boolean",
      "createdBy": {
        "id": "uuid",
        "firstName": "string",
        "lastName": "string"
      },
      "createdAt": "datetime"
    }
  ],
  "pagination": {
    "currentPage": "integer",
    "totalPages": "integer",
    "totalItems": "integer",
    "itemsPerPage": "integer"
  }
}
```

#### POST /items
- **Description**: Create new item
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "householdId": "uuid",
  "categoryId": "integer",
  "name": "string",
  "description": "string",
  "yearsValue": "integer",
  "monthsValue": "integer",
  "weeksValue": "integer",
  "daysValue": "integer",
  "lastDate": "date",
  "priority": "string",
  "notes": "string"
}
```
- **Response**: 
  - **201**: Created item object with calculated next due date
  - **400**: `{ "error": "Item limit reached for free plan" }`
  - **422**: `{ "error": "At least one interval value required" }`

#### GET /items/{id}
- **Description**: Get specific item
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: Item object
  - **404**: `{ "error": "Item not found" }`
  - **403**: `{ "error": "Access denied" }`

#### PUT /items/{id}
- **Description**: Update item
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**: Same as POST /items (partial updates allowed)
- **Response**: 
  - **200**: Updated item object
  - **403**: `{ "error": "Can only edit assigned items or admin access required" }`

#### DELETE /items/{id}
- **Description**: Soft delete item
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Item deleted" }`
  - **403**: `{ "error": "Can only delete assigned items or admin access required" }`

### Tasks

#### GET /tasks
- **Description**: Get tasks for user's households
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `itemId` (optional)
  - `assignedTo` (optional)
  - `status` (optional)
  - `priority` (optional)
  - `dueDateFrom` (optional)
  - `dueDateTo` (optional)
  - `sortBy` (optional: dueDate, priority, createdAt)
  - `sortOrder` (optional: asc, desc, default: asc)
  - `page` (optional, default: 1)
  - `limit` (optional, default: 20, max: 100)
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "uuid",
      "item": {
        "id": "uuid",
        "name": "string",
        "category": {
          "name": "string",
          "categoryType": {
            "name": "string"
          }
        }
      },
      "householdId": "uuid",
      "assignedTo": {
        "id": "uuid",
        "firstName": "string",
        "lastName": "string"
      },
      "dueDate": "date",
      "title": "string",
      "description": "string",
      "status": "string",
      "priority": "string",
      "urgencyStatus": "string",
      "createdAt": "datetime"
    }
  ],
  "pagination": {
    "currentPage": "integer",
    "totalPages": "integer",
    "totalItems": "integer",
    "itemsPerPage": "integer"
  }
}
```

#### POST /tasks
- **Description**: Create new task
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "itemId": "uuid",
  "assignedTo": "uuid",
  "dueDate": "date",
  "title": "string",
  "description": "string",
  "priority": "string"
}
```
- **Response**: 
  - **201**: Created task object
  - **400**: `{ "error": "Validation failed" }`

#### GET /tasks/{id}
- **Description**: Get specific task
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: Task object
  - **404**: `{ "error": "Task not found" }`
  - **403**: `{ "error": "Access denied" }`

#### PUT /tasks/{id}
- **Description**: Update task
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**: Same as POST /tasks (partial updates allowed)
- **Response**: 
  - **200**: Updated task object
  - **403**: `{ "error": "Can only edit assigned tasks or admin access required" }`

#### POST /tasks/{id}/complete
- **Description**: Mark task as completed
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "completionNotes": "string",
  "completionDate": "date"
}
```
- **Response**: 
  - **200**: 
```json
{
  "message": "Task completed",
  "completedTask": {
    "id": "uuid",
    "completionDate": "date",
    "completionNotes": "string"
  },
  "nextTask": {
    "id": "uuid",
    "dueDate": "date",
    "title": "string"
  }
}
```

#### POST /tasks/{id}/postpone
- **Description**: Postpone task to new date
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "newDueDate": "date",
  "postponeReason": "string"
}
```
- **Response**: 
  - **200**: Updated task object
  - **403**: `{ "error": "Can only postpone assigned tasks" }`

#### DELETE /tasks/{id}
- **Description**: Cancel/delete task
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: `generateNew` (boolean, optional, default: false)
- **Response**: 
  - **200**: `{ "message": "Task cancelled", "newTask": {} }` (if generateNew=true)

### Tasks History (Premium)

#### GET /tasks-history
- **Description**: Get completed tasks history (premium only)
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `itemId` (optional)
  - `completedBy` (optional)
  - `completionDateFrom` (optional)
  - `completionDateTo` (optional)
  - `page` (optional, default: 1)
  - `limit` (optional, default: 20, max: 100)
- **Response**: 
  - **200**: Array of completed tasks with completion details
  - **403**: `{ "error": "Premium subscription required" }`

### Dashboard

#### GET /dashboard/upcoming-tasks
- **Description**: Get upcoming tasks for dashboard
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `days` (optional, default: 7)
  - `householdId` (optional)
- **Response**: 
  - **200**: 
```json
{
  "data": [
    {
      "id": "uuid",
      "dueDate": "date",
      "title": "string",
      "urgencyStatus": "string",
      "item": {
        "name": "string",
        "category": {
          "name": "string",
          "categoryType": {
            "name": "string"
          }
        }
      },
      "assignedTo": {
        "firstName": "string",
        "lastName": "string"
      },
      "priority": "string"
    }
  ],
  "summary": {
    "overdue": "integer",
    "today": "integer",
    "thisWeek": "integer"
  }
}
```

#### GET /dashboard/statistics
- **Description**: Get household statistics
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: `householdId` (optional)
- **Response**: 
  - **200**: 
```json
{
  "items": {
    "total": "integer",
    "byCategory": [
      {
        "categoryName": "string",
        "count": "integer"
      }
    ]
  },
  "tasks": {
    "pending": "integer",
    "overdue": "integer",
    "completedThisMonth": "integer"
  },
  "planUsage": {
    "itemsUsed": "integer",
    "itemsLimit": "integer",
    "membersUsed": "integer",
    "membersLimit": "integer"
  }
}
```

## 3. Authentication and Authorization

### Authentication Mechanism
- **JWT Bearer Token**: All protected endpoints require `Authorization: Bearer {token}` header
- **Token Expiration**: 30 days for regular sessions, 7 days for "remember me" disabled
- **Refresh Strategy**: Automatic token refresh on successful API calls within expiration window

### Authorization Rules
1. **Household-based Access**: Users can only access data for households they belong to
2. **Role-based Permissions**:
   - **Admin**: Full CRUD access to all household data
   - **Member**: Read access to all household data, write access only to assigned items/tasks
   - **Dashboard**: Read-only access to tasks and items
3. **Row Level Security**: Enforced at database level via Supabase RLS policies
4. **Premium Features**: Additional check for active subscription status

### Security Headers
- All responses include security headers: `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`
- CORS configured for frontend domain only
- Rate limiting: 100 requests per minute per user

## 4. Validation and Business Logic

### Validation Rules

#### Items
- **name**: Required, max 100 characters
- **priority**: Must be one of: 'low', 'medium', 'high'
- **interval**: At least one of yearsValue, monthsValue, weeksValue, or daysValue must be > 0
- **lastDate**: Cannot be in the future
- **Freemium Limit**: Maximum 5 items per household on free plan

#### Tasks
- **title**: Required, max 200 characters
- **dueDate**: Required, must be valid date
- **status**: Must be one of: 'pending', 'completed', 'postponed', 'cancelled'
- **priority**: Must be one of: 'low', 'medium', 'high'
- **assignedTo**: Must be a member of the task's household

#### Household Members
- **role**: Must be one of: 'admin', 'member', 'dashboard'
- **Unique Constraint**: One user can have only one active membership per household
- **Admin Requirement**: At least one admin must remain in each household
- **Freemium Limit**: Maximum 3 members per household on free plan
// End of Selection
```

### Business Logic Implementation

#### Automatic Task Generation
- When item is created: Calculate next due date based on lastDate + interval
- When task is completed: Generate new task with dueDate = completionDate + interval
- Task generation creates recurring tasks by default (isRecurring = true)

#### Freemium Limitations
- API enforces limits before creation operations
- Returns HTTP 400 with upgrade suggestion when limits exceeded
- Limits checked: household members (3), items (5)

#### Soft Delete Pattern
- All DELETE operations set `deleted_at` timestamp instead of physical deletion
- All GET operations filter out records where `deleted_at IS NOT NULL`
- Cascade behavior preserves referential integrity for soft-deleted records

#### Premium Feature Access
- Endpoints marked as premium return HTTP 403 if user's household lacks active subscription
- Premium features: tasks history, advanced analytics, unlimited limits


#### Data Isolation
- All data operations automatically filter by user's household memberships
- Cross-household access is strictly forbidden via RLS policies
- User can belong to multiple households but data remains isolated per household
