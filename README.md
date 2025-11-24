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
- **Angular 20** - Modern web framework for building responsive user interfaces
- **PrimeNG** - Comprehensive UI component library

### Backend
- **.NET 8** - Robust framework for business logic and API development
- **ASP.NET Core** - Web API framework

### Database
- **Supabase** - Open-source PostgreSQL database with built-in authentication
- **PostgreSQL** - Reliable relational database

### Infrastructure & DevOps
- **GitHub Actions** - CI/CD pipeline automation
- **AWS** - Cloud hosting platform
- **Docker** - Containerization for deployment

## Getting Started Locally

### Prerequisites

Make sure you have the following installed:
- [Node.js](https://nodejs.org/) (v18 or higher)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (optional, for containerization)

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
   cd ../backend
   dotnet restore
   ```

4. **Configure Environment Variables**
   ```bash
   # Copy environment template files
   cp .env.example .env
   # Configure your Supabase connection and other settings
   ```

5. **Database Setup**
   - Create a Supabase project
   - Configure connection string in your environment variables
   - Run migrations (if available)

### Running the Application

1. **Start the Backend API**
   ```bash
   cd backend
   dotnet run
   ```

2. **Start the Frontend (in a new terminal)**
   ```bash
   cd frontend
   npm start
   ```

3. **Access the Application**
   - Frontend: `http://localhost:4200`
   - Backend API: `http://localhost:8080`

## Available Scripts

### Frontend (Angular)
- `npm start` - Start development server
- `npm run build` - Build for production
- `npm run test` - Run unit tests
- `npm run lint` - Run linting
- `npm run e2e` - Run end-to-end tests

### Backend (.NET)
- `dotnet run` - Start development server
- `dotnet build` - Build the project
- `dotnet test` - Run unit tests
- `dotnet publish` - Publish for deployment

### Docker
- `docker-compose up` - Start all services with Docker
- `docker-compose down` - Stop all services

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
- [ ] Backend API development
- [ ] Frontend application development
- [x] Database schema implementation
- [x] Authentication system
- [ ] Core CRUD operations
- [ ] Email notification system
- [ ] Document upload functionality
- [ ] Testing suite
- [ ] Deployment pipeline

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
