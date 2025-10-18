# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Homely is a household management application for tracking service appointments, maintenance tasks, and family visits. The application uses a three-tier architecture with:
- **Frontend**: Angular 20 with PrimeNG (planned, not yet implemented)
- **Backend**: ASP.NET Core with .NET 9 REST API
- **Database**: Supabase (PostgreSQL) with Row Level Security

The project implements a freemium business model with usage limits for free users and premium features for subscribers.

## Technology Stack

### Backend (.NET 9)
- **Framework**: ASP.NET Core Web API
- **Database Access**: Entity Framework Core with Repository and Unit of Work patterns
- **Authentication**: Supabase Auth integration with JWT
- **Architecture**: Domain-Driven Design (DDD) with bounded contexts
- **API Documentation**: Swagger/OpenAPI

### Frontend (Planned)
- **Framework**: Angular 20 standalone components
- **UI Library**: PrimeNG
- **State Management**: Angular Signals
- **Styling**: Modern Angular patterns with @if, @for, @switch

### Database
- **Provider**: Supabase (PostgreSQL)
- **Security**: Row Level Security (RLS) policies
- **Migrations**: Located in `database/supabase/migrations/`

### Infrastructure
- **CI/CD**: GitHub Actions
- **Cloud**: AWS (planned)
- **Containerization**: Docker (planned)

## Development Commands

### Backend Development

**Location**: `backend/HomelyApi/`

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the development server
cd Homely.API
dotnet run

# Run tests (when implemented)
dotnet test

# Publish for deployment
dotnet publish -c Release
```

The backend API runs on `https://localhost:5000` by default.

### Database Development

**Location**: `database/`

```bash
# Install Supabase CLI (first time only)
cd database
npm install

# Link to Supabase project
npx supabase link --project-ref <your-project-ref>

# Create a new migration
npx supabase migration new <migration_name>

# Apply migrations
npx supabase db push

# Reset database (development only)
npx supabase db reset

# Generate TypeScript types from database
npx supabase gen types typescript --local > types/database.ts
```

### Frontend Development (When Implemented)

```bash
cd frontend
npm install
npm start           # Start dev server (localhost:4200)
npm run build       # Production build
npm run test        # Run unit tests
npm run lint        # Run linting
npm run e2e         # Run e2e tests
```

## Architecture and Code Structure

### Database Schema

The database uses a multi-tenant architecture with households as the primary isolation boundary:

**Core Tables**:
- `auth.users` - Managed by Supabase Auth (DO NOT create manually)
- `households` - Household entities with subscription plans
- `household_members` - User-household relationship with roles (admin, member, dashboard)
- `plan_types` - Subscription plan definitions
- `category_types` - High-level categories (technical inspections, waste collection, medical visits)
- `categories` - Specific categories within types
- `items` - Devices and visits to track
- `tasks` - Scheduled appointments with due dates
- `tasks_history` - Completed tasks archive (premium feature)

**Key Relationships**:
- Users can belong to multiple households
- Each household has a subscription plan with limits (free: 3 members, 5 items)
- Items generate recurring tasks based on intervals (years, months, weeks, days)
- All tables implement soft delete pattern (`deleted_at` column)

**Security**:
- Row Level Security (RLS) enforces data isolation between households
- Users can only access data for households they're members of
- Role-based access control (admin, member, dashboard roles)

### Backend Architecture Patterns

**Domain-Driven Design**:
- Define bounded contexts for authentication, household management, task scheduling
- Use ubiquitous language aligned with business terminology
- Create rich domain models with behavior, not just data
- Implement value objects for concepts without identity
- Use aggregates to enforce consistency boundaries
- Trigger Supabase operations via domain events (asynchronously)

**Entity Framework Guidelines**:
- Use Repository and Unit of Work patterns
- Implement eager loading with `Include()` to avoid N+1 queries
- Apply `AsNoTracking()` for read-only queries
- Use compiled queries for frequently executed operations
- Synchronize EF migrations with Supabase schema

**ASP.NET Best Practices**:
- Use Minimal APIs for simple endpoints in .NET 6+
- Implement Mediator pattern with MediatR for request handling
- Use Dependency Injection with appropriate lifetimes:
  - Scoped: Request-specific services
  - Singleton: Stateless services
- Integrate Supabase SDK via typed HTTP clients
- Implement Supabase JWT authentication using ASP.NET middleware

**Supabase Integration**:
- Use official Supabase .NET SDK or REST API via typed HTTP clients
- Store API keys and secrets in ASP.NET configuration (user secrets for development)
- Handle network retries with resilient policies (e.g., Polly)
- Map Supabase users to local domain entities when additional business logic required
- Use `auth.uid()` function in RLS policies for user identification

### Frontend Architecture (When Implemented)

**Angular Modern Patterns**:
- Use standalone components instead of NgModules
- Implement Signals for state management
- Use `inject()` function instead of constructor injection
- Use control flow with @if, @for, @switch instead of directives
- Implement OnPush change detection strategy
- Use functional guards and resolvers
- Leverage deferrable views for loading states

### API Structure

The REST API follows RESTful conventions with resource-based endpoints:

**Authentication**: `/auth/*` - Registration, login, logout, password reset
**Households**: `/households/*` - Household CRUD operations
**Household Members**: `/households/{id}/members/*` - Member management
**Items**: `/items/*` - Device and visit management
**Tasks**: `/tasks/*` - Task scheduling and completion
**Dashboard**: `/dashboard/*` - Aggregated dashboard data

**Key Endpoints**:
- `POST /auth/register` - New user registration
- `POST /auth/login` - User authentication (returns JWT)
- `GET /dashboard/upcoming-tasks?days=7` - Dashboard with upcoming tasks
- `POST /tasks/{id}/complete` - Mark task complete, generates next recurring task
- `POST /tasks/{id}/postpone` - Postpone task with reason
- `GET /tasks-history` - Premium feature for completed tasks

**Authentication**: All protected endpoints require `Authorization: Bearer {token}` header

**Validation**: Enforced on both client and server side

**Business Rules**:
- Free plan limits: 3 household members, 5 items
- Task generation: Automatic based on item intervals
- Premium features: Task history, unlimited items/members

## Coding Standards

### Backend (.NET)

From `.cursor/rules/backend-coding.mdc`:

**Entity Framework**:
- Use Repository and Unit of Work patterns
- Implement eager loading with `Include()` to avoid N+1 queries
- Apply `AsNoTracking()` for read-only queries
- Synchronize EF Core migrations with Supabase schema

**ASP.NET**:
- Use Minimal APIs for simple endpoints
- Implement Mediator pattern with MediatR
- Use API Controllers with model binding and validation
- Apply proper response caching with cache profiles
- Implement consistent exception handling with middleware

**Swagger**:
- Define comprehensive schemas for all request/response objects
- Use semantic versioning in API paths
- Provide detailed descriptions and examples
- Document authentication schemes
- Use tags to group related endpoints

**Supabase Integration**:
- Configure connection details using ASP.NET configuration providers
- Never store API keys or JWT secrets in code
- Handle network retries with resilient policies
- Map Supabase users to local domain entities when needed
- Integrate Supabase JWT validation middleware

### Frontend (Angular)

From `.cursor/rules/frontend-coding.mdc`:

**Angular Coding Standards**:
- Use standalone components, directives, and pipes
- Implement Signals for state management
- Use `inject()` function instead of constructor injection
- Use @if, @for, @switch instead of *ngIf, *ngFor, *ngSwitch
- Leverage functional guards and resolvers
- Use deferrable views for improved loading states
- Implement OnPush change detection strategy
- Use TypeScript decorators with explicit visibility modifiers
- Leverage Angular CLI for schematics and code generation

### DevOps

From `.cursor/rules/devops.mdc`:

**GitHub Actions**:
- Check for `package.json`, `.nvmrc`, `.env.example` in project root
- Verify branch name with `git branch -a | cat` (main vs master)
- Use `env:` variables attached to jobs instead of global workflow
- Use `npm ci` for Node-based dependency setup
- Extract common steps into composite actions
- Use latest major versions of public actions

**Docker**:
- Use multi-stage builds for smaller production images
- Implement layer caching strategies
- Use non-root users in containers

**AWS**:
- Use Infrastructure as Code (IaC) with AWS CDK or CloudFormation
- Implement least privilege for IAM roles and policies
- Use managed services when possible

## Important Notes

### Supabase Authentication
The `auth.users` table is automatically managed by Supabase - **DO NOT create it manually**. User data should be accessed directly from the `auth.users` table when needed.

### Freemium Limitations
Free plan enforces:
- Maximum 3 household members
- Maximum 5 items (devices/visits)
- No access to task history and analytics

Premium plan removes these limits and adds advanced features.

### Soft Delete Pattern
All main tables use soft delete (`deleted_at` timestamp). Always filter out soft-deleted records in queries and indexes.

### Security
- Row Level Security (RLS) is enabled on all main tables
- Users can only access data for households they belong to
- Never expose Supabase API keys or secrets in client code
- Use JWT tokens with proper expiration (30 days)

### Task Generation Logic
When a task is completed:
1. Mark current task as completed
2. Calculate next due date: completion_date + interval
3. Create new recurring task automatically
4. Archive to `tasks_history` if premium user

## Project Status

Currently in MVP development phase. The backend API structure and database schema are defined, but implementation is in progress. Frontend Angular application is not yet started.

**Completed**:
- ✅ Project planning and PRD documentation
- ✅ Technology stack selection
- ✅ Database schema design
- ✅ API endpoint planning
- ✅ Supabase migrations created

**In Progress**:
- 🚧 Backend API development (.NET 9)
- 🚧 Database implementation

**Not Started**:
- ⏳ Frontend application development
- ⏳ Authentication system implementation
- ⏳ Testing suite
- ⏳ CI/CD pipeline
- ⏳ Deployment
