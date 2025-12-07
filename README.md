#  <img  src="frontend/public/homely-icon.ico" alt="Homely Icon">  Homely

A comprehensive web application for managing household service appointments and family visits, designed to help homeowners efficiently organize and track various home maintenance tasks and family schedules.

## Table of Contents

- [Project Description](#project-description)
- [Tech Stack](#tech-stack)
- [Getting Started Locally](#getting-started-locally)
- [Available Scripts](#available-scripts)
- [Project Scope](#project-scope)
- [Project Status](#project-status)
- [License](#license)

## Project Description

Homely is a centralized web system that enables household members to effectively manage service appointments for home appliances and family visits, eliminating the problem of missed appointments and inefficient management of domestic responsibilities.

### Key Features

- **Service Management**: Track technical inspections, waste collection, and medical visits
- **Automated Notifications**: Email reminders to prevent missed appointments
- **Document Storage**: Store invoices, manuals, and warranties in context
- **Multi-user Support**: Clear responsibility assignment among household members
- **Freemium Model**: Basic features free with premium upgrades available

### Target Audience

Homeowners managing household operations who need to organize and monitor various appointments related to home maintenance and family member visits.

## Tech Stack

### Frontend
- **Angular 19** - Modern web framework for building responsive user interfaces
- **PrimeNG** - Comprehensive UI component library
- **Playwright** - End-to-end testing framework

### Backend
- **.NET 9** - Robust framework for business logic and API development
- **ASP.NET Core** - Web API framework

### Database
- **Supabase** - Open-source PostgreSQL database with built-in authentication
- **PostgreSQL** - Reliable relational database

### Infrastructure & DevOps
- **GitHub Actions** - CI/CD pipeline automation (backend, frontend, database, E2E tests)
- **AWS** - Cloud hosting platform (planned)
- **Docker** - Containerization for E2E testing and deployment

## Getting Started Locally

### Prerequisites

Make sure you have the following installed:
- [Node.js](https://nodejs.org/) (v18 or higher)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (for E2E tests)
- [Supabase CLI](https://supabase.com/docs/guides/cli) (for database management)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/homely.git
   cd homely
   ```

2. **Setup Frontend**
   ```bash
   cd frontend
   npm install
   ```

3. **Setup Backend**
   ```bash
   cd backend/HomelyApi
   dotnet restore
   ```

4. **Configure Environment Variables**

   Create `appsettings.Local.json` in `backend/HomelyApi/Homely.API/` with your Supabase credentials:
   ```json
   {
     "Supabase": {
       "Url": "https://your-project.supabase.co",
       "Key": "your-anon-key"
     },
     "Jwt": {
       "Secret": "your-jwt-secret",
       "ValidIssuer": "https://your-project.supabase.co/auth/v1"
     }
   }
   ```

5. **Database Setup**
   ```bash
   cd database
   npm install

   # Link to your Supabase project
   npx supabase link --project-ref <your-project-ref>

   # Apply migrations
   npx supabase db push

   # Configure backend connection in appsettings.Local.json
   ```

### Running the Application

1. **Start the Backend API**
   ```bash
   cd backend/HomelyApi/Homely.API
   dotnet run
   ```

2. **Start the Frontend (in a new terminal)**
   ```bash
   cd frontend
   npm start
   ```

3. **Access the Application**
   - Frontend: `http://localhost:4200`
   - Backend API: `http://localhost:5000` (or `https://localhost:5443`)

## Available Scripts

### Frontend (Angular)
- `npm start` - Start development server (local configuration)
- `npm run start:e2e` - Start development server (E2E configuration)
- `npm run build` - Build for production
- `npm run test` - Run unit tests
- `npm run e2e` - Run end-to-end tests with Playwright
- `npm run e2e:ui` - Run E2E tests in UI mode
- `npm run e2e:full` - Run full E2E test suite with Docker

### Backend (.NET)
- `dotnet run` - Start development server
- `dotnet build` - Build the project
- `dotnet test` - Run unit tests
- `dotnet publish` - Publish for deployment

### Database (Supabase)
- `cd database && npx supabase link` - Link to Supabase project
- `npx supabase migration new <name>` - Create a new migration
- `npx supabase db push` - Apply migrations to database
- `npx supabase db reset` - Reset database (development only)
- `npx supabase status` - Check Supabase project status

### Docker (E2E Testing)
- `cd frontend && npm run e2e:docker:start` - Start E2E test environment
- `npm run e2e:docker:stop` - Stop E2E test environment
- `npm run e2e:docker:clean` - Clean up E2E containers and volumes

## Project Scope

### MVP Features ✅

The first version includes four core categories:
- **Technical Inspections** - Home appliance maintenance tracking
- **Waste Collection** - Garbage collection schedule management  
- **Medical Visits** - Family member appointment tracking

### Core Functionality
- User authentication and household management
- Device and visit management with automated scheduling
- Email notification system
- Document storage and management
- Responsive web interface with calendar and dashboard views
- Freemium model with usage limits

### Future Enhancements 🚀

Post-MVP planned features:
- Additional categories (plants, insurance, recurring payments, pets)
- Native mobile applications with push notifications
- External calendar integrations (Google Calendar, Outlook)
- OCR document processing
- Predictive maintenance recommendations
- Service provider marketplace
- Advanced analytics and cost reporting

### Out of Scope

- Native mobile apps (web-responsive only for MVP)
- AI/ML features (OCR, prediction algorithms)
- Social features and community aspects
- Smart home integrations
- Automatic payment processing

## Project Status

🚧 **In Development** - MVP Phase

### Current Progress
- [x] Project planning and requirements documentation
- [x] Technology stack selection
- [x] Backend API development (controllers, services, repositories)
- [x] Frontend application development (Angular 19 with PrimeNG)
- [x] Database schema implementation
- [x] Authentication system (JWT-based with Supabase)
- [x] Core CRUD operations (tasks, events, categories, households)
- [x] Testing suite (unit, integration, and E2E tests with Playwright)
- [x] CI/CD pipeline (GitHub Actions workflows)
- [ ] Email notification system
- [ ] Document upload functionality
- [ ] Production deployment

### Success Metrics
- 1000+ registered users within 3 months
- 40% retention rate after 3 months
- 60% of users active after 30 days
- 3% conversion rate from free to premium
- Average 3+ devices/visits per active user

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Contributing**: We welcome contributions! Please read our contributing guidelines before submitting pull requests.

**Support**: For questions and support, please open an issue in the GitHub repository.

**Security**: For security concerns, please email security@homely-app.com instead of opening a public issue.
