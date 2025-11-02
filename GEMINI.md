# Homely - Project Analysis

## Project Overview

This is a full-stack web application for managing household service appointments and family visits. It is designed to help homeowners efficiently organize and track various home maintenance tasks and family schedules.

**Key Technologies:**

*   **Frontend:** Angular 20 with PrimeNG for the UI components.
*   **Backend:** .NET 8 (ASP.NET Core) for the Web API.
*   **Database:** Supabase (PostgreSQL) with a well-defined schema, including tables for households, items, tasks, and user management.
*   **Infrastructure:** The project is set up with GitHub Actions for CI/CD and is intended for deployment on AWS using Docker.

**Architecture:**

The project follows a standard monolithic architecture with a clear separation of concerns between the frontend, backend, and database. The backend exposes a RESTful API that the frontend consumes. The database schema is well-designed with tables, views, triggers, and row-level security policies to ensure data integrity and security.

## Building and Running

### Prerequisites

*   Node.js (v18 or higher)
*   .NET 8 SDK
*   Docker (optional)

### Frontend

To build and run the frontend:

```bash
cd frontend
npm install
npm start
```

The frontend will be available at `http://localhost:4200`.

### Backend

To build and run the backend:

```bash
cd backend/HomelyApi
dotnet restore
dotnet run --project Homely.API
```

The backend API will be available at `http://localhost:5000`.

### Database

The project uses Supabase for the database. To set up the database:

1.  Create a Supabase project.
2.  Configure the connection string in the backend's `appsettings.json` file.
3.  Run the database migrations located in the `database/supabase/migrations` directory.

## Development Conventions

*   **Coding Style:** The project uses the standard coding styles for Angular (TypeScript) and .NET (C#).
*   **Testing:** The project has a testing suite for both the frontend and backend.
    *   Frontend tests can be run with `npm test`.
    *   Backend tests can be run with `dotnet test`.
*   **Database Migrations:** Database changes are managed through SQL migration files.
*   **Security:** The application uses JWT for authentication and has row-level security policies in the database to enforce data access rules.
