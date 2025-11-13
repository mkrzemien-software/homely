# REST API Plan

## 1. Resources

- **Profiles** - User profile data (`user_profiles` table)
- **Households** - Household management (`households` table)
- **HouseholdMembers** - Household membership with roles (`household_members` table)
- **PlanTypes** - Subscription plan definitions (`plan_types` table)
- **CategoryTypes** - High-level categories (`category_types` table)
- **Categories** - Specific categories within types (subcategories) (`categories` table)
- **Tasks** - Task templates/definitions (`tasks` table)
- **Events** - Scheduled appointments/occurrences (`events` table)
- **EventsHistory** - Completed events archive (`events_history` table)
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
- **Description**: Get all available category types
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

#### GET /category-types/{id}
- **Description**: Get specific category type by ID
- **Response**:
  - **200**: Category type object
  - **404**: `{ "error": "Category type not found" }`

#### POST /category-types
- **Description**: Create new category type
- **Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "sortOrder": "integer",
  "isActive": "boolean"
}
```
- **Response**:
  - **201**: Created category type object
  - **400**: `{ "error": "Validation failed" }`

#### PUT /category-types/{id}
- **Description**: Update existing category type
- **Request Body**: Same as POST (all fields optional for partial updates)
- **Response**:
  - **200**: Updated category type object
  - **404**: `{ "error": "Category type not found" }`
  - **400**: `{ "error": "Validation failed" }`

#### DELETE /category-types/{id}
- **Description**: Soft delete category type
- **Response**:
  - **200**: `{ "success": true, "message": "Category type deleted successfully" }`
  - **404**: `{ "error": "Category type not found" }`

### Categories

#### GET /categories
- **Description**: Get categories (subcategories) by type
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

### Tasks

#### GET /tasks
- **Description**: Get task templates for user's households
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `categoryId` (optional) - subcategory filter
  - `priority` (optional)
  - `hasInterval` (optional: true/false)
  - `isActive` (optional, default: true)
  - `sortBy` (optional: name, priority, createdAt)
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

#### POST /tasks
- **Description**: Create new task template
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
  "priority": "string",
  "notes": "string"
}
```
- **Response**: 
  - **201**: Created task object
  - **400**: `{ "error": "Task limit reached for free plan (maximum 5 tasks)" }`
  - **422**: `{ "error": "Validation failed" }`

#### GET /tasks/{id}
- **Description**: Get specific task template
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: Task object
  - **404**: `{ "error": "Task not found" }`
  - **403**: `{ "error": "Access denied" }`

#### PUT /tasks/{id}
- **Description**: Update task template
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**: Same as POST /tasks (partial updates allowed)
- **Response**: 
  - **200**: Updated task object
  - **403**: `{ "error": "Admin access required" }`

#### DELETE /tasks/{id}
- **Description**: Soft delete task template
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Task deleted" }`
  - **403**: `{ "error": "Admin access required" }`
  - **400**: `{ "error": "Cannot delete task with active events. Archive or complete all events first." }`

### Events

#### GET /events
- **Description**: Get events (scheduled occurrences) for user's households
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `taskId` (optional) - filter by task template
  - `assignedTo` (optional)
  - `status` (optional: pending, completed, postponed, cancelled)
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
      "task": {
        "id": "uuid",
        "name": "string",
        "category": {
          "name": "string",
          "categoryType": {
            "name": "string"
          }
        },
        "interval": {
          "years": "integer",
          "months": "integer",
          "weeks": "integer",
          "days": "integer"
        }
      },
      "householdId": "uuid",
      "assignedTo": {
        "id": "uuid",
        "firstName": "string",
        "lastName": "string"
      },
      "dueDate": "date",
      "status": "string",
      "priority": "string",
      "urgencyStatus": "string",
      "notes": "string",
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

#### POST /events
- **Description**: Create new event (manually schedule occurrence from task template)
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "taskId": "uuid",
  "assignedTo": "uuid",
  "dueDate": "date",
  "priority": "string",
  "notes": "string"
}
```
- **Response**: 
  - **201**: Created event object
  - **400**: `{ "error": "Validation failed" }`
  - **404**: `{ "error": "Task template not found" }`

#### GET /events/{id}
- **Description**: Get specific event
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: Event object with full task details
  - **404**: `{ "error": "Event not found" }`
  - **403**: `{ "error": "Access denied" }`

#### PUT /events/{id}
- **Description**: Update event
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**: Same as POST /events (partial updates allowed)
- **Response**: 
  - **200**: Updated event object
  - **403**: `{ "error": "Can only edit assigned events or admin access required" }`

#### POST /events/{id}/complete
- **Description**: Mark event as completed (automatically generates next event if task has interval)
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
  "message": "Event completed",
  "completedEvent": {
    "id": "uuid",
    "completionDate": "date",
    "completionNotes": "string",
    "status": "completed"
  },
  "nextEvent": {
    "id": "uuid",
    "dueDate": "date",
    "assignedTo": {
      "id": "uuid",
      "firstName": "string",
      "lastName": "string"
    }
  }
}
```
- **Note**: If task template has interval, system automatically creates next event. If no interval, nextEvent will be null.

#### POST /events/{id}/postpone
- **Description**: Postpone event to new date
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "newDueDate": "date",
  "postponeReason": "string"
}
```
- **Response**: 
  - **200**: Updated event object with status "postponed"
  - **403**: `{ "error": "Can only postpone assigned events" }`

#### POST /events/{id}/cancel
- **Description**: Cancel event (does not generate new event)
- **Headers**: `Authorization: Bearer {token}`
- **Request Body**:
```json
{
  "cancelReason": "string"
}
```
- **Response**: 
  - **200**: `{ "message": "Event cancelled", "eventId": "uuid" }`
  - **403**: `{ "error": "Can only cancel assigned events or admin access required" }`

#### DELETE /events/{id}
- **Description**: Delete event permanently
- **Headers**: `Authorization: Bearer {token}`
- **Response**: 
  - **200**: `{ "message": "Event deleted" }`
  - **403**: `{ "error": "Admin access required" }`

### Events History (Premium)

#### GET /events-history
- **Description**: Get completed events history (premium only)
- **Headers**: `Authorization: Bearer {token}`
- **Query Parameters**: 
  - `householdId` (optional)
  - `taskId` (optional) - filter by task template
  - `completedBy` (optional)
  - `completionDateFrom` (optional)
  - `completionDateTo` (optional)
  - `sortBy` (optional: completionDate, dueDate)
  - `sortOrder` (optional: asc, desc)
  - `page` (optional, default: 1)
  - `limit` (optional, default: 20, max: 100)
- **Response**: 
  - **200**: Array of completed events with completion details
  - **403**: `{ "error": "Premium subscription required" }`

### Dashboard

#### GET /dashboard/upcoming-events
- **Description**: Get upcoming events for dashboard
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
      "urgencyStatus": "string",
      "task": {
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
      "priority": "string",
      "status": "string"
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
  "tasks": {
    "total": "integer",
    "byCategory": [
      {
        "categoryName": "string",
        "count": "integer"
      }
    ]
  },
  "events": {
    "pending": "integer",
    "overdue": "integer",
    "completedThisMonth": "integer"
  },
  "planUsage": {
    "tasksUsed": "integer",
    "tasksLimit": "integer",
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

#### Tasks (Templates)
- **name**: Required, max 100 characters
- **categoryId**: Required, must be valid subcategory
- **priority**: Must be one of: 'low', 'medium', 'high'
- **interval**: Optional, but if provided at least one of yearsValue, monthsValue, weeksValue, or daysValue must be > 0
- **Freemium Limit**: Maximum 5 tasks per household on free plan

#### Events (Occurrences)
- **taskId**: Required, must reference existing task template
- **assignedTo**: Required, must be a member of the event's household
- **dueDate**: Required, must be valid date
- **status**: Must be one of: 'pending', 'completed', 'postponed', 'cancelled'
- **priority**: Must be one of: 'low', 'medium', 'high'
- **No Limit**: Unlimited events on both free and premium plans

#### Household Members
- **role**: Must be one of: 'admin', 'member', 'dashboard'
- **Unique Constraint**: One user can have only one active membership per household
- **Admin Requirement**: At least one admin must remain in each household
- **Freemium Limit**: Maximum 3 members per household on free plan
// End of Selection
```

### Business Logic Implementation

#### Manual Event Creation
- When task template is created: No automatic event generation
- User must manually create first event via POST /events with taskId reference
- Event inherits priority from task template by default (can be overridden)

#### Automatic Event Generation on Completion
- When event is completed via POST /events/{id}/complete:
  - If task template has interval → automatically generate next event
  - Next event dueDate = completionDate + interval (from task template)
  - Next event inherits assignedTo from completed event (can be changed)
  - Next event inherits priority from task template
- If task template has no interval (one-time task) → no automatic generation

#### Freemium Limitations
- API enforces limits before creation operations
- Returns HTTP 400 with upgrade suggestion when limits exceeded
- Limits checked: household members (3 on free plan), tasks (5 on free plan)
- Events have no limits on any plan

#### Soft Delete Pattern
- All DELETE operations set `deleted_at` timestamp instead of physical deletion
- All GET operations filter out records where `deleted_at IS NOT NULL`
- Cascade behavior preserves referential integrity for soft-deleted records
- Cannot delete task template if it has active (non-completed) events

#### Premium Feature Access
- Endpoints marked as premium return HTTP 403 if user's household lacks active subscription
- Premium features: events history, advanced analytics, unlimited tasks

#### Data Isolation
- All data operations automatically filter by user's household memberships
- Cross-household access is strictly forbidden via RLS policies
- User can belong to multiple households but data remains isolated per household
