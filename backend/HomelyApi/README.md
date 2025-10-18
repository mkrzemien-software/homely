# Homely API - Authentication Implementation

## Overview
This is the backend API for the Homely application - a home management system for tracking service appointments and visits. The API is built using ASP.NET Core (.NET 8) and integrates with Supabase for authentication.

## Features Implemented

### Authentication Endpoints
- **POST `/api/auth/login`** - User login with email/password
- **POST `/api/auth/refresh`** - Refresh access token
- **POST `/api/auth/logout`** - User logout
- **GET `/api/auth/me`** - Get current user information
- **GET `/health`** - Health check endpoint

### Security Features
- JWT Bearer authentication using Supabase Auth
- CORS policy configuration
- Security headers (HSTS, XSS protection, etc.)
- Model validation with Polish error messages
- Structured API responses

### Documentation
- Swagger/OpenAPI documentation available at `/swagger`
- JWT authentication integration in Swagger UI
- XML documentation comments

## Configuration

### appsettings.json (Production)
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-anon-key"
  },
  "JwtSettings": {
    "ValidIssuer": "https://your-project.supabase.co/auth/v1",
    "ValidAudience": "authenticated", 
    "Secret": "your-jwt-secret"
  }
}
```

### appsettings.Development.json (Local Development)
```json
{
  "Supabase": {
    "Url": "http://127.0.0.1:54321",
    "Key": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6ImFub24iLCJleHAiOjE5ODM4MTI5OTZ9.CRXP1A7WOeoJeXxjNni43kdQwgnWNReilDMblYTn_I0"
  },
  "JwtSettings": {
    "ValidIssuer": "http://127.0.0.1:54321/auth/v1",
    "ValidAudience": "authenticated",
    "Secret": "super-secret-jwt-token-with-at-least-32-characters-long"
  }
}
```

## Project Structure

```
Homely.API/
├── Controllers/
│   ├── AuthController.cs          # Authentication endpoints
│   └── WeatherForecastController.cs # Legacy test controller
├── Models/
│   └── DTOs/
│       ├── LoginRequestDto.cs      # Login request model
│       ├── LoginResponseDto.cs     # Login response model  
│       └── ApiResponseDto.cs       # Standard API response wrapper
├── Services/
│   ├── IAuthService.cs            # Authentication service interface
│   └── AuthService.cs             # Supabase auth implementation
├── Configuration/
│   ├── SupabaseSettings.cs        # Supabase configuration model
│   └── JwtSettings.cs             # JWT configuration model
├── Program.cs                     # Application startup and configuration
└── HomelyAPI.http                # HTTP test requests
```

## API Endpoints

### Authentication

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123", 
  "rememberMe": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGci...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": "user-uuid",
      "email": "user@example.com", 
      "name": "User Name",
      "emailConfirmed": true,
      "createdAt": "2023-10-01T10:00:00Z"
    }
  },
  "statusCode": 200
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "errorMessage": "Nieprawidłowy email lub hasło",
  "statusCode": 401
}
```

#### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your_refresh_token"
}
```

#### Get Current User
```http
GET /api/auth/me
Authorization: Bearer your_access_token
```

#### Logout
```http
POST /api/auth/logout  
Authorization: Bearer your_access_token
```

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Supabase project (local or hosted)

### Running Locally

1. **Start Supabase locally** (if using local development):
   ```bash
   cd database
   npx supabase start
   ```

2. **Restore packages:**
   ```bash
   cd backend/HomelyApi/Homely.API
   dotnet clean
   dotnet restore
   ```

3. **Run the API:**
   ```bash
   dotnet run
   ```

4. **Access Swagger documentation:**
   - Open `https://localhost:5001/swagger` in your browser

### Supabase C# Library Update

The project has been updated to use **Supabase 1.1.1** (latest version as of July 2024). Key improvements:
- Enhanced stability and performance
- Better .NET compatibility 
- Updated namespace structure
- Improved error handling

### Testing
Use the provided `HomelyAPI.http` file to test endpoints, or use Swagger UI.

## Security Considerations

### Implemented
- JWT token validation
- HTTPS enforcement  
- CORS policy
- Security headers
- Input validation
- Structured error handling

### PRD Compliance
- ✅ Email/password authentication via Supabase
- ✅ Session management (30-day expiry configurable)
- ✅ GDPR compliance ready (via Supabase)
- ✅ Polish error messages for user-facing errors
- ✅ Structured logging for audit trails

### Production Recommendations
- Update CORS origins to match your frontend domain
- Use proper secrets management for JWT keys
- Enable rate limiting for auth endpoints
- Set up monitoring and alerts
- Configure proper logging levels

## Error Handling

All API responses follow a consistent format:

```json
{
  "success": boolean,
  "data": object | null,
  "errorMessage": string | null,
  "errors": string[] | null,
  "statusCode": number
}
```

Common error scenarios:
- **400 Bad Request**: Invalid input data
- **401 Unauthorized**: Invalid credentials or expired token
- **500 Internal Server Error**: Server-side errors

## Next Steps

To complete the Homely application according to the PRD:

1. **User Registration** - Add signup endpoint
2. **Password Reset** - Add password reset flow
3. **Household Management** - Add household creation and member management
4. **Device/Visit Management** - Add CRUD operations for devices and visits
5. **Scheduling System** - Add appointment scheduling logic
6. **Premium Features** - Add subscription and premium feature checks

## Dependencies

- **Supabase** (1.1.1) - Authentication and database - Latest stable version from [NuGet](https://www.nuget.org/packages/supabase)
- **Microsoft.AspNetCore.Authentication.JwtBearer** (9.0.0) - JWT authentication
- **Swashbuckle.AspNetCore** (7.0.0) - API documentation
- **System.IdentityModel.Tokens.Jwt** (8.1.2) - JWT token handling
- **Microsoft.AspNetCore.OpenApi** (9.0.0) - OpenAPI support
