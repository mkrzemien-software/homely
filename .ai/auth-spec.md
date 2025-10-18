# Specyfikacja Architektury Systemu Autentykacji - Homely

## 1. WPROWADZENIE

### 1.1 Cel dokumentu
Niniejszy dokument przedstawia szczegółową architekturę systemu rejestracji, logowania i odzyskiwania hasła dla aplikacji Homely - systemu zarządzania terminami gospodarstwa domowego.

### 1.2 Kontekst techniczny
Aplikacja wykorzystuje następujący stos technologiczny:
- **Frontend**: Angular 20 z komponentami PrimeNG
- **Backend**: ASP.NET Core (.NET 8) z Supabase Auth (już zaimplementowany)
- **Baza danych**: PostgreSQL przez Supabase z RLS (Row Level Security)
- **Autentykacja**: Supabase Auth z JWT tokenami
- **CI/CD**: GitHub Actions z deployment na AWS (Docker containers)

### 1.3 Zakres funkcjonalności
- Rejestracja nowych użytkowników z weryfikacją email
- Logowanie i wylogowywanie użytkowników
- Reset i zmiana hasła
- Zarządzanie sesjami użytkowników
- System ról i uprawnień (Administrator, Domownik, Dashboard)
- Tworzenie i zarządzanie gospodarstwem domowym
- Onboarding nowych użytkowników

---

## 2. ARCHITEKTURA INTERFEJSU UŻYTKOWNIKA

### 2.1 Struktura aplikacji frontendowej

#### 2.1.1 Organizacja modułów Angular
```
src/
├── app/
│   ├── modules/              # Moduły funkcjonalne Angular
│   │   ├── auth/
│   │   │   ├── pages/
│   │   │   │   ├── login.component.ts         # Strona logowania
│   │   │   │   ├── register.component.ts      # Strona rejestracji
│   │   │   │   ├── forgot-password.component.ts # Reset hasła
│   │   │   │   ├── reset-password.component.ts  # Ustawienie nowego hasła
│   │   │   │   └── verify-email.component.ts    # Weryfikacja emaila
│   │   │   ├── components/
│   │   │   │   ├── login-form.component.ts    # Formularz logowania
│   │   │   │   ├── register-form.component.ts # Formularz rejestracji
│   │   │   │   └── password-reset-form.component.ts # Formularze reset
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts              # Ochrona tras
│   │   │   │   └── role.guard.ts              # Ochrona ról
│   │   │   ├── services/
│   │   │   │   └── auth.service.ts            # Serwis autentykacji
│   │   │   └── auth-routing.module.ts         # Routing modułu
│   │   ├── onboarding/
│   │   │   ├── components/
│   │   │   │   ├── welcome.component.ts       # Powitanie
│   │   │   │   ├── household-setup.component.ts # Tworzenie gospodarstwa
│   │   │   │   └── profile-setup.component.ts   # Uzupełnienie profilu
│   │   │   └── onboarding-routing.module.ts
│   │   ├── dashboard/
│   │   │   ├── pages/
│   │   │   │   ├── dashboard.component.ts     # Główny dashboard
│   │   │   │   └── settings.component.ts      # Ustawienia
│   │   │   └── dashboard-routing.module.ts
│   │   └── shared/
│   │       ├── components/
│   │       │   ├── layouts/
│   │       │   │   ├── auth-layout.component.ts    # Layout auth
│   │       │   │   ├── dashboard-layout.component.ts # Layout dashboard  
│   │       │   │   └── public-layout.component.ts   # Layout publiczny
│   │       │   └── ui/                        # Komponenty UI (+ PrimeNG)
│   │       │       ├── button/
│   │       │       ├── input/
│   │       │       ├── alert/
│   │       │       └── loading-spinner/
│   │       └── services/
│   │           ├── api.service.ts             # HTTP Client wrapper
│   │           └── notification.service.ts    # Toast notifications
│   ├── core/
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts            # JWT token interceptor
│   │   │   └── error.interceptor.ts           # Error handling
│   │   └── models/
│   │       ├── user.model.ts
│   │       ├── household.model.ts
│   │       └── auth-response.model.ts
│   └── app-routing.module.ts                  # Main routing
```

#### 2.1.2 Podział odpowiedzialności Angular

**Page Components (Smart Components)**:
- Obsługa routingu Angular i nawigacji między stronami
- Zarządzanie stanem strony i lifecycle hooks
- Komunikacja z serwisami Angular (dependency injection)
- Orchestracja działania komponentów prezentacyjnych
- Obsługa Angular Guards (CanActivate, CanLoad)
- Meta tags i SEO przez Angular Universal (opcjonalnie)

**Presentation Components (Dumb Components)**:
- Interaktywne formularze z Angular Reactive Forms
- Walidacja w czasie rzeczywistym z Angular Validators
- Integracja z PrimeNG components (p-button, p-inputText, p-toast)
- Input/Output komunikacja z parent components
- Emisja eventów bez logiki biznesowej
- Reusable UI elements zgodne z design system

**Angular Services**:
- AuthService - zarządzanie stanem autentykacji
- ApiService - komunikacja HTTP z backendem
- NotificationService - toast notifications (PrimeNG MessageService)
- HttpInterceptors - automatic JWT token attachment

### 2.2 Szczegółowa specyfikacja stron

#### 2.2.1 Strona logowania (`/auth/login`)

**Struktura strony**:
- Sprawdzenie czy użytkownik już zalogowany
- Przekierowanie do dashboard jeśli sesja aktywna
- Renderowanie layoutu autentykacji z formularzem logowania
- Obsługa parametrów URL (redirectTo, error messages)

**Komponent LoginFormComponent**:
- **Inputs**: `@Input() redirectTo?: string` - URL przekierowania po logowaniu  
- **Reactive Form**: 
  ```typescript
  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [false]
  });
  ```
- **Component State**:
  - `isLoading: boolean = false`
  - `generalError: string | null = null`
- **PrimeNG Integration**:
  - `p-inputText` dla pól email i password
  - `p-checkbox` dla "Zapamiętaj mnie"
  - `p-button` z loading spinner
  - `p-message` dla error display
- **Funkcjonalności**:
  - Walidacja w czasie rzeczywistym z Angular Validators
  - Submit przez AuthService.login()
  - Router navigation po udanym logowaniu
  - Toast notifications dla błędów

#### 2.2.2 Strona rejestracji (`/auth/register`)

**Komponent RegisterFormComponent**:
- **Reactive Form z Custom Validators**:
  ```typescript
  registerForm = this.fb.group({
    email: ['', [Validators.required, Validators.email], [this.emailAvailabilityValidator()]],
    password: ['', [Validators.required, this.passwordStrengthValidator()]],
    confirmPassword: ['', [Validators.required]],
    name: ['', [Validators.required, Validators.minLength(2), this.polishNameValidator()]],
    gdprConsent: [false, [Validators.requiredTrue]]
  }, { validators: this.passwordMatchValidator });
  ```
- **Component State**:
  - `isLoading: boolean = false`
  - `currentStep: 'form' | 'verification' = 'form'`
- **PrimeNG Integration**:
  - `p-inputText` z walidacją w czasie rzeczywistym
  - `p-password` z strength indicator
  - `p-checkbox` dla zgody RODO
  - `p-progressSpinner` podczas ładowania
  - `p-steps` dla multi-step flow
- **Custom Validators Angular**:
  - `emailAvailabilityValidator()` - async validator
  - `passwordStrengthValidator()` - cyfra + znak specjalny
  - `polishNameValidator()` - polskie znaki diakrytyczne
  - `passwordMatchValidator` - zgodność haseł

#### 2.2.3 Strona resetu hasła (`/auth/forgot-password`)

**Komponent ForgotPasswordForm**:
- **State lokalny**:
  - `email: string`
  - `isSubmitted: boolean`
  - `isLoading: boolean`
  - `error?: string`
- **Przepływ resetowania**:
  1. Wprowadzenie adresu email
  2. Walidacja formatu email
  3. Wysłanie żądania do API backend
  4. Wyświetlenie komunikatu potwierdzającego wysłanie linku

#### 2.2.4 Strona ustawienia nowego hasła (`/auth/reset-password`)

**Logika strony**:
- Sprawdzenie ważności tokena reset hasła z URL
- Walidacja tokena przez API backend
- Przekierowanie do forgot-password jeśli token nieważny
- Renderowanie formularza jeśli token poprawny

**Komponent ResetPasswordForm**:
- **Props**: `token: string` - token z URL
- **State lokalny**:
  - `formData: { password: string, confirmPassword: string }`
  - `errors: Record<string, string>`
  - `isLoading: boolean`
- **Funkcjonalności**:
  - Walidacja siły hasła
  - Potwierdzenie zgodności haseł
  - Submit z obsługą błędów serwera

### 2.3 System nawigacji i ochrony tras

#### 2.3.1 Angular Guards
```typescript
@Injectable()
export class AuthGuard implements CanActivate, CanLoad {
  constructor(
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    return this.checkAuth(route);
  }

  canLoad(route: Route): Observable<boolean> {
    return this.checkAuth();
  }

  private checkAuth(route?: ActivatedRouteSnapshot): Observable<boolean> {
    return this.authService.isAuthenticated$.pipe(
      map(isAuth => {
        if (!isAuth) {
          const redirectUrl = route?.url?.join('/') || '';
          this.router.navigate(['/auth/login'], { 
            queryParams: { redirectTo: redirectUrl } 
          });
          return false;
        }
        return true;
      })
    );
  }
}

@Injectable()
export class RoleGuard implements CanActivate {
  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    const requiredRole = route.data['role'] as string;
    return this.authService.hasRole(requiredRole);
  }
}
```

**Angular Routing Configuration**:
```typescript
const routes: Routes = [
  {
    path: 'dashboard',
    loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule),
    canLoad: [AuthGuard],
    canActivate: [AuthGuard]
  },
  {
    path: 'household',
    loadChildren: () => import('./modules/household/household.module').then(m => m.HouseholdModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { role: 'administrator' }
  },
  {
    path: 'auth',
    loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule),
    canActivate: [GuestGuard] // Redirect if already logged in
  }
];
```

### 2.4 Obsługa stanów i błędów

#### 2.4.1 Komunikaty błędów (polskie)
```tsx
const ERROR_MESSAGES = {
  INVALID_EMAIL: 'Nieprawidłowy format adresu email',
  PASSWORD_TOO_SHORT: 'Hasło musi mieć minimum 8 znaków',
  PASSWORD_MISMATCH: 'Hasła nie są identyczne',
  REQUIRED_FIELD: 'To pole jest wymagane',
  LOGIN_FAILED: 'Nieprawidłowy email lub hasło',
  NETWORK_ERROR: 'Błąd połączenia. Sprawdź internet i spróbuj ponownie',
  TOKEN_EXPIRED: 'Sesja wygasła. Zaloguj się ponownie',
  EMAIL_NOT_VERIFIED: 'Potwierdź adres email przed logowaniem'
};
```

#### 2.4.2 Toast Notification System
```tsx
interface ToastContextType {
  showSuccess: (message: string) => void;
  showError: (message: string) => void;
  showWarning: (message: string) => void;
}
```

---

## 3. LOGIKA BACKENDOWA

### 3.1 Rozszerzenie istniejących endpointów

#### 3.1.1 Nowe endpointy do zaimplementowania

**AuthController - rozszerzenia**:
```csharp
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Istniejące: Login, Refresh, Logout, GetCurrentUser
    
    [HttpPost("register")]
    public async Task<ApiResponseDto<RegisterResponseDto>> Register(RegisterRequestDto request);
    
    [HttpPost("forgot-password")]
    public async Task<ApiResponseDto<bool>> ForgotPassword(ForgotPasswordRequestDto request);
    
    [HttpPost("reset-password")]
    public async Task<ApiResponseDto<bool>> ResetPassword(ResetPasswordRequestDto request);
    
    [HttpPost("verify-email")]
    public async Task<ApiResponseDto<bool>> VerifyEmail(VerifyEmailRequestDto request);
    
    [HttpPost("resend-verification")]
    public async Task<ApiResponseDto<bool>> ResendVerification(ResendVerificationRequestDto request);
    
    [HttpGet("validate-reset-token/{token}")]
    public async Task<ApiResponseDto<bool>> ValidateResetToken(string token);
}
```

**HouseholdController** (nowy):
```csharp
[Route("api/[controller]")]
[Authorize]
public class HouseholdController : ControllerBase
{
    [HttpPost]
    public async Task<ApiResponseDto<HouseholdDto>> CreateHousehold(CreateHouseholdRequestDto request);
    
    [HttpGet("{id}")]
    public async Task<ApiResponseDto<HouseholdDto>> GetHousehold(Guid id);
    
    [HttpPut("{id}")]
    public async Task<ApiResponseDto<HouseholdDto>> UpdateHousehold(Guid id, UpdateHouseholdRequestDto request);
    
    [HttpPost("{id}/members")]
    public async Task<ApiResponseDto<HouseholdMemberDto>> AddMember(Guid id, AddHouseholdMemberRequestDto request);
    
    [HttpDelete("{householdId}/members/{memberId}")]
    public async Task<ApiResponseDto<bool>> RemoveMember(Guid householdId, Guid memberId);
    
    [HttpPut("{householdId}/members/{memberId}/role")]
    public async Task<ApiResponseDto<bool>> UpdateMemberRole(Guid householdId, Guid memberId, UpdateMemberRoleRequestDto request);
}
```

### 3.2 Nowe modele danych DTO

#### 3.2.1 Registration DTOs
```csharp
public class RegisterRequestDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; }
    
    [Required, MinLength(8)]
    public string Password { get; set; }
    
    [Required, MinLength(2), MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    public bool GdprConsent { get; set; }
    
    public string? PreferredLanguage { get; set; } = "pl";
}

public class RegisterResponseDto
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public bool EmailVerificationRequired { get; set; }
    public string? VerificationMessage { get; set; }
}
```

#### 3.2.2 Password Reset DTOs
```csharp
public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; }
}

public class ResetPasswordRequestDto
{
    [Required]
    public string Token { get; set; }
    
    [Required, MinLength(8)]
    public string NewPassword { get; set; }
}
```

#### 3.2.3 Household DTOs
```csharp
public class CreateHouseholdRequestDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
}

public class HouseholdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }
    public string PlanType { get; set; }
    public string SubscriptionStatus { get; set; }
    public List<HouseholdMemberDto> Members { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HouseholdMemberDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string? Color { get; set; }
    public DateTime JoinedAt { get; set; }
}
```

### 3.3 Services - nowe implementacje

#### 3.3.1 IUserService
```csharp
public interface IUserService
{
    Task<ApiResponseDto<RegisterResponseDto>> RegisterUserAsync(RegisterRequestDto request);
    Task<ApiResponseDto<bool>> VerifyEmailAsync(string token);
    Task<ApiResponseDto<bool>> ResendVerificationEmailAsync(string email);
    Task<ApiResponseDto<UserProfileDto>> GetUserProfileAsync(string userId);
    Task<ApiResponseDto<UserProfileDto>> UpdateUserProfileAsync(string userId, UpdateUserProfileDto request);
}
```

#### 3.3.2 IHouseholdService
```csharp
public interface IHouseholdService
{
    Task<ApiResponseDto<HouseholdDto>> CreateHouseholdAsync(string userId, CreateHouseholdRequestDto request);
    Task<ApiResponseDto<HouseholdDto>> GetUserHouseholdAsync(string userId);
    Task<ApiResponseDto<bool>> AddMemberAsync(Guid householdId, string inviterId, AddHouseholdMemberRequestDto request);
    Task<ApiResponseDto<bool>> RemoveMemberAsync(Guid householdId, Guid memberId, string requesterId);
    Task<ApiResponseDto<bool>> UpdateMemberRoleAsync(Guid householdId, Guid memberId, string newRole, string requesterId);
    Task<bool> CheckUserPermissionAsync(string userId, Guid householdId, string requiredPermission);
}
```

#### 3.3.3 IPasswordService
```csharp
public interface IPasswordService
{
    Task<ApiResponseDto<bool>> SendPasswordResetEmailAsync(string email);
    Task<ApiResponseDto<bool>> ResetPasswordAsync(string token, string newPassword);
    Task<bool> ValidateResetTokenAsync(string token);
    Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
```

### 3.4 Walidacje biznesowe

#### 3.4.1 CustomValidationAttributes
```csharp
[AttributeUsage(AttributeTargets.Property)]
public class PolishNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        // Walidacja polskich znaków, minimum 2 znaki, tylko litery
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class SecurePasswordAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        // Minimum 8 znaków, cyfra, znak specjalny, litera
    }
}
```

### 3.5 Obsługa błędów i wyjątków

#### 3.5.1 Custom Exceptions
```csharp
public class HomelyBusinessException : Exception
{
    public string ErrorCode { get; }
    public int HttpStatusCode { get; }
    
    public HomelyBusinessException(string errorCode, string message, int statusCode = 400) 
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = statusCode;
    }
}

public static class ErrorCodes
{
    public const string USER_ALREADY_EXISTS = "USER_ALREADY_EXISTS";
    public const string HOUSEHOLD_MEMBER_LIMIT_REACHED = "HOUSEHOLD_MEMBER_LIMIT_REACHED";
    public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
    public const string INVALID_RESET_TOKEN = "INVALID_RESET_TOKEN";
    public const string EMAIL_NOT_VERIFIED = "EMAIL_NOT_VERIFIED";
}
```

#### 3.5.2 Global Exception Handler
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (HomelyBusinessException ex)
        {
            await HandleHomelyExceptionAsync(context, ex);
        }
        catch (GotrueException ex)
        {
            await HandleSupabaseExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }
}
```

---

## 4. SYSTEM AUTENTYKACJI

### 4.1 Integracja Supabase Auth z frontendem

#### 4.1.1 Angular Supabase Service
```typescript
@Injectable({ providedIn: 'root' })
export class SupabaseService {
  private supabase: SupabaseClient;

  constructor() {
    this.supabase = createClient(
      environment.supabaseUrl,
      environment.supabaseAnonKey,
      {
        auth: {
          autoRefreshToken: true,
          persistSession: true,
          detectSessionInUrl: true
        }
      }
    );
  }

  get auth() {
    return this.supabase.auth;
  }

  get client() {
    return this.supabase;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private sessionSubject = new BehaviorSubject<Session | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public session$ = this.sessionSubject.asObservable();
  public isAuthenticated$ = this.session$.pipe(map(session => !!session));

  constructor(
    private supabase: SupabaseService,
    private http: HttpClient,
    private router: Router,
    private messageService: MessageService
  ) {
    // Listen for auth changes
    this.supabase.auth.onAuthStateChange((event, session) => {
      this.sessionSubject.next(session);
      this.currentUserSubject.next(session?.user || null);
      
      if (event === 'SIGNED_OUT') {
        this.router.navigate(['/auth/login']);
      }
    });
  }

  async signIn(email: string, password: string, rememberMe = false): Promise<AuthResponse> {
    const { data, error } = await this.supabase.auth.signInWithPassword({
      email,
      password
    });

    if (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Błąd logowania',
        detail: this.translateAuthError(error.message)
      });
      throw error;
    }

    return data;
  }

  async signUp(email: string, password: string, name: string): Promise<AuthResponse> {
    return this.supabase.auth.signUp({
      email,
      password,
      options: {
        data: { name }
      }
    });
  }

  async signOut(): Promise<void> {
    await this.supabase.auth.signOut();
  }

  async resetPassword(email: string): Promise<void> {
    await this.supabase.auth.resetPasswordForEmail(email);
  }

  hasRole(role: string): Observable<boolean> {
    return this.currentUser$.pipe(
      map(user => user?.user_metadata?.household_role === role)
    );
  }

  private translateAuthError(error: string): string {
    const errorMap: Record<string, string> = {
      'Invalid login credentials': 'Nieprawidłowy email lub hasło',
      'Email not confirmed': 'Potwierdź adres email przed logowaniem',
      'Too many requests': 'Zbyt wiele prób logowania. Spróbuj później'
    };
    return errorMap[error] || 'Wystąpił błąd podczas logowania';
  }
}
```

#### 4.1.3 Session Management po stronie serwera
**Funkcjonalności server-side**:
- Walidacja JWT tokenów na serwerze
- Sprawdzanie stanu sesji przed renderowaniem stron
- Bezpieczne przekierowania na podstawie stanu autentykacji
- Obsługa ciasteczek sesji z odpowiednimi flagami bezpieczeństwa
- Server-side session refresh gdy to możliwe

### 4.2 Role-based Access Control (RBAC)

#### 4.2.1 Enum ról
```csharp
public enum HouseholdRole
{
    Administrator = 1,
    Domownik = 2,
    Dashboard = 3
}

public static class Permissions
{
    public const string MANAGE_HOUSEHOLD = "manage_household";
    public const string MANAGE_MEMBERS = "manage_members";
    public const string VIEW_ALL_ITEMS = "view_all_items";
    public const string EDIT_ALL_ITEMS = "edit_all_items";
    public const string EDIT_ASSIGNED_ITEMS = "edit_assigned_items";
    public const string MANAGE_SUBSCRIPTION = "manage_subscription";
    public const string VIEW_REPORTS = "view_reports";
}
```

#### 4.2.2 Authorization Attributes
```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireHouseholdRoleAttribute : Attribute, IAuthorizationRequirement
{
    public HouseholdRole RequiredRole { get; }
    public string? RequiredPermission { get; }
    
    public RequireHouseholdRoleAttribute(HouseholdRole role, string? permission = null)
    {
        RequiredRole = role;
        RequiredPermission = permission;
    }
}
```

#### 4.2.3 Frontend Role Guards
```tsx
interface RoleGuardProps {
  allowedRoles: ('administrator' | 'domownik' | 'dashboard')[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export const RoleGuard: React.FC<RoleGuardProps> = ({ 
  allowedRoles, 
  children, 
  fallback 
}) => {
  const { user } = useAuth();
  const userRole = user?.user_metadata?.household_role;
  
  if (!allowedRoles.includes(userRole)) {
    return fallback || <UnauthorizedMessage />;
  }
  
  return <>{children}</>;
};
```

### 4.3 Onboarding Flow

#### 4.3.1 Multi-step Onboarding
```tsx
type OnboardingStep = 'welcome' | 'household' | 'profile' | 'complete';

interface OnboardingState {
  currentStep: OnboardingStep;
  completedSteps: OnboardingStep[];
  householdData?: Partial<CreateHouseholdRequestDto>;
  profileData?: Partial<UserProfileDto>;
}

export const OnboardingWizard: React.FC = () => {
  const [state, setState] = useState<OnboardingState>({
    currentStep: 'welcome',
    completedSteps: []
  });
  
  const steps = [
    { id: 'welcome', component: WelcomeStep, title: 'Witamy w Homely!' },
    { id: 'household', component: HouseholdStep, title: 'Utwórz gospodarstwo' },
    { id: 'profile', component: ProfileStep, title: 'Twój profil' },
    { id: 'complete', component: CompleteStep, title: 'Gotowe!' }
  ];
};
```

#### 4.3.2 Welcome Step Component
```tsx
const WelcomeStep: React.FC<OnboardingStepProps> = ({ onNext }) => {
  return (
    <div className="onboarding-step">
      <h1>Witamy w Homely!</h1>
      <p>Pomożemy Ci skonfigurować Twoje gospodarstwo domowe w kilku prostych krokach.</p>
      
      <div className="features-preview">
        <FeatureCard 
          icon="calendar" 
          title="Zarządzaj terminami"
          description="Nigdy nie przegap ważnego przeglądu czy wizyty"
        />
        <FeatureCard 
          icon="users" 
          title="Koordynacja rodziny"
          description="Wszyscy członkowie będą na bieżąco"
        />
        <FeatureCard 
          icon="bell" 
          title="Przypomnienia"
          description="Dostaniesz powiadomienie przed terminem"
        />
      </div>
      
      <Button onClick={onNext} size="lg">
        Rozpocznij konfigurację
      </Button>
    </div>
  );
};
```

### 4.4 Security Features

#### 4.4.1 CSRF Protection
```csharp
public class AntiForgeryMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsStateChangingRequest(context.Request))
        {
            await ValidateAntiForgeryTokenAsync(context);
        }
        
        await next(context);
    }
}
```

#### 4.4.2 Rate Limiting
```csharp
public class AuthRateLimitingMiddleware
{
    private readonly Dictionary<string, RateLimit> _rateLimits = new()
    {
        { "login", new RateLimit { RequestsPerMinute = 5, WindowMinutes = 1 } },
        { "register", new RateLimit { RequestsPerMinute = 3, WindowMinutes = 5 } },
        { "forgot-password", new RateLimit { RequestsPerMinute = 2, WindowMinutes = 15 } }
    };
}
```

#### 4.4.3 Audit Logging
```csharp
public class AuditLogService
{
    public async Task LogAuthEventAsync(string userId, string eventType, string? details = null, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            EventType = eventType,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };
        
        await _dbContext.AuditLogs.AddAsync(auditLog);
        await _dbContext.SaveChangesAsync();
    }
}
```

---

## 5. PRZEPŁYW DANYCH I INTEGRACJE

### 5.1 Synchronizacja stanu autentykacji

#### 5.1.1 Zarządzanie stanem frontend
```typescript
// Store dla globalnego stanu autentykacji
interface AuthStore {
  user: User | null;
  session: Session | null;
  household: Household | null;
  isLoading: boolean;
  
  // Actions
  setUser: (user: User | null) => void;
  setSession: (session: Session | null) => void;
  setHousehold: (household: Household | null) => void;
  clearAuth: () => void;
  
  // Async actions
  refreshUserData: () => Promise<void>;
  switchHousehold: (householdId: string) => Promise<void>;
}
```

#### 5.1.2 Synchronizacja Server-Client
**Mechanizm hydration stanu**:
- Initial state loading podczas pierwszego załadowania strony
- Sprawdzenie stanu sesji na serwerze przed renderowaniem
- Przekazanie initial auth state do klienta
- Hydration globalnego store z danymi server-side
- Graceful handling różnic między stanem server/client

### 5.2 Email Integration

#### 5.2.1 Email Templates
```csharp
public class EmailTemplateService
{
    public async Task<string> GetWelcomeEmailAsync(string userName, string verificationLink)
    {
        return await RenderTemplateAsync("welcome", new
        {
            UserName = userName,
            VerificationLink = verificationLink,
            AppName = "Homely"
        });
    }
    
    public async Task<string> GetPasswordResetEmailAsync(string userName, string resetLink)
    {
        return await RenderTemplateAsync("password-reset", new
        {
            UserName = userName,
            ResetLink = resetLink,
            ExpirationHours = 24
        });
    }
    
    public async Task<string> GetHouseholdInvitationEmailAsync(string inviterName, string householdName, string invitationLink)
    {
        return await RenderTemplateAsync("household-invitation", new
        {
            InviterName = inviterName,
            HouseholdName = householdName,
            InvitationLink = invitationLink
        });
    }
}
```

### 5.3 Database Schema Extensions

#### 5.3.1 Dodatkowe tabele dla autentykacji
```sql
-- Tabela dla tokenów reset hasła
CREATE TABLE password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES auth.users(id),
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    used_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Tabela dla zaproszeń do gospodarstwa
CREATE TABLE household_invitations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    household_id UUID NOT NULL REFERENCES households(id),
    email VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'domownik',
    invited_by UUID NOT NULL,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    accepted_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Tabela dla audit logów
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES auth.users(id),
    event_type VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

#### 5.3.2 RLS Policies Extension
```sql
-- Policy dla household_members - użytkownik widzi tylko członków swojego gospodarstwa
CREATE POLICY "Users can view household members of their household"
ON household_members FOR SELECT
USING (
    household_id IN (
        SELECT household_id 
        FROM household_members 
        WHERE user_id = auth.uid()
    )
);

-- Policy dla audit_logs - użytkownik widzi tylko swoje logi
CREATE POLICY "Users can view their own audit logs"
ON audit_logs FOR SELECT
USING (user_id = auth.uid());
```

---

## 6. TESTING STRATEGY

### 6.1 Frontend Testing

#### 6.1.1 Angular Unit Tests (Jasmine + Karma)
```typescript
// src/app/modules/auth/components/login-form.component.spec.ts
describe('LoginFormComponent', () => {
  let component: LoginFormComponent;
  let fixture: ComponentFixture<LoginFormComponent>;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['signIn']);

    await TestBed.configureTestingModule({
      declarations: [LoginFormComponent],
      imports: [ReactiveFormsModule, PrimeNgModule],
      providers: [
        { provide: AuthService, useValue: authSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginFormComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  it('should display validation errors for empty fields', () => {
    // Submit empty form
    component.onSubmit();
    fixture.detectChanges();

    expect(component.loginForm.get('email')?.hasError('required')).toBe(true);
    expect(component.loginForm.get('password')?.hasError('required')).toBe(true);
  });

  it('should call AuthService.signIn with form data', async () => {
    authService.signIn.and.returnValue(Promise.resolve({} as any));

    // Fill form
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: 'password123',
      rememberMe: false
    });

    await component.onSubmit();

    expect(authService.signIn).toHaveBeenCalledWith(
      'test@example.com',
      'password123',
      false
    );
  });
});
```

#### 6.1.2 E2E Tests (Protractor/Cypress)
```typescript
// e2e/auth-flow.e2e-spec.ts
describe('Authentication Flow', () => {
  it('should complete user registration', () => {
    cy.visit('/auth/register');
    
    // Fill PrimeNG components
    cy.get('p-inputText[formControlName="email"] input')
      .type('test@example.com');
    cy.get('p-password[formControlName="password"] input')
      .type('SecurePass123!');
    cy.get('p-password[formControlName="confirmPassword"] input')
      .type('SecurePass123!');
    cy.get('p-inputText[formControlName="name"] input')
      .type('Jan Kowalski');
    cy.get('p-checkbox[formControlName="gdprConsent"]')
      .click();
    
    // Submit using PrimeNG button
    cy.get('p-button[type="submit"]').click();
    
    // Verify redirection
    cy.url().should('include', '/auth/verify-email');
    cy.contains('Sprawdź swoją skrzynkę email').should('be.visible');
  });
});
```

### 6.2 Backend Testing

#### 6.2.1 Integration Tests
```csharp
[TestClass]
public class AuthControllerTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Register_ValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            Name = "Jan Kowalski",
            GdprConsent = true
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        
        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<RegisterResponseDto>>();
        result.Success.Should().BeTrue();
        result.Data.EmailVerificationRequired.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task Register_ExistingEmail_ReturnsError()
    {
        // Test duplicate email registration
    }
}
```

---

## 7. DEPLOYMENT I KONFIGURACJA

### 7.1 Environment Configuration

#### 7.1.1 Angular Environment Configuration
```typescript
// src/environments/environment.ts (development)
export const environment = {
  production: false,
  supabaseUrl: 'http://localhost:54321',
  supabaseAnonKey: 'eyJ...',
  apiBaseUrl: 'http://localhost:5000/api',
  appName: 'Homely'
};

// src/environments/environment.prod.ts (production)
export const environment = {
  production: true,
  supabaseUrl: 'https://your-project.supabase.co',
  supabaseAnonKey: 'eyJ...',
  apiBaseUrl: 'https://api.homely.app/api',
  appName: 'Homely'
};
```

#### 7.1.2 Docker Configuration
```dockerfile
# Frontend Angular Dockerfile
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build --prod

FROM nginx:alpine
COPY --from=builder /app/dist/homely-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

#### 7.1.3 Backend (.NET 8 appsettings.json)
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-anon-key",
    "ServiceKey": "your-service-key"
  },
  "EmailSettings": {
    "Provider": "Supabase",
    "FromAddress": "noreply@homely.app",
    "FromName": "Homely"
  },
  "SecuritySettings": {
    "PasswordResetTokenExpiryHours": 24,
    "EmailVerificationTokenExpiryHours": 72,
    "MaxLoginAttempts": 5,
    "AccountLockoutMinutes": 15
  }
}
```

### 7.2 CI/CD Pipeline

#### 7.2.1 GitHub Actions Workflow dla AWS Deployment
```yaml
name: Deploy Homely App to AWS

on:
  push:
    branches: [main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET 8
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Run backend tests
        run: dotnet test backend/

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install dependencies
        run: npm ci
      - name: Run Angular tests
        run: npm run test:ci
      - name: Run E2E tests
        run: npm run e2e:ci

  build-and-deploy:
    needs: [test-backend, test-frontend]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: eu-west-1

      - name: Build Angular Frontend
        run: |
          npm ci
          npm run build --prod
          
      - name: Build and Push Frontend Docker Image
        run: |
          docker build -t homely-frontend .
          docker tag homely-frontend:latest ${{ secrets.AWS_ECR_URI }}/homely-frontend:latest
          aws ecr get-login-password | docker login --username AWS --password-stdin ${{ secrets.AWS_ECR_URI }}
          docker push ${{ secrets.AWS_ECR_URI }}/homely-frontend:latest

      - name: Build and Push Backend Docker Image  
        run: |
          cd backend/HomelyApi
          docker build -t homely-backend .
          docker tag homely-backend:latest ${{ secrets.AWS_ECR_URI }}/homely-backend:latest
          docker push ${{ secrets.AWS_ECR_URI }}/homely-backend:latest

      - name: Deploy to AWS ECS
        run: |
          aws ecs update-service --cluster homely-cluster --service homely-frontend-service --force-new-deployment
          aws ecs update-service --cluster homely-cluster --service homely-backend-service --force-new-deployment
```

---

## 8. MONITORING I ANALYTICS

### 8.1 Metryki uwierzytelniania

#### 8.1.1 Key Performance Indicators
- **Registration Conversion Rate**: % użytkowników kończących rejestrację
- **Email Verification Rate**: % potwierdzonych adresów email
- **Login Success Rate**: % udanych logowań
- **Password Reset Usage**: liczba resetów hasła
- **Session Duration**: średni czas sesji użytkownika
- **Onboarding Completion Rate**: % użytkowników kończących onboarding

#### 8.1.2 Error Tracking
```csharp
public class AuthMetricsService
{
    public async Task TrackRegistrationAttempt(string email, bool success, string? errorCode = null)
    {
        await _analytics.TrackEvent("registration_attempt", new
        {
            email_domain = GetEmailDomain(email),
            success = success,
            error_code = errorCode,
            timestamp = DateTime.UtcNow
        });
    }
    
    public async Task TrackLoginAttempt(string email, bool success, string? errorCode = null)
    {
        await _analytics.TrackEvent("login_attempt", new
        {
            email_domain = GetEmailDomain(email),
            success = success,
            error_code = errorCode,
            timestamp = DateTime.UtcNow
        });
    }
}
```

### 8.2 Security Monitoring

#### 8.2.1 Suspicious Activity Detection
```csharp
public class SecurityMonitoringService
{
    public async Task<bool> DetectSuspiciousActivity(string ipAddress, string userAgent, string action)
    {
        var recentAttempts = await GetRecentAttemptsAsync(ipAddress, TimeSpan.FromMinutes(10));
        
        // Zbyt wiele prób z tego IP
        if (recentAttempts.Count > 20)
        {
            await _alertService.SendSecurityAlertAsync($"Multiple attempts from IP: {ipAddress}");
            return true;
        }
        
        // Różne User Agents z tego IP
        var uniqueUserAgents = recentAttempts.Select(a => a.UserAgent).Distinct().Count();
        if (uniqueUserAgents > 5)
        {
            await _alertService.SendSecurityAlertAsync($"Multiple user agents from IP: {ipAddress}");
            return true;
        }
        
        return false;
    }
}
```

---

## 9. PODSUMOWANIE I NASTĘPNE KROKI

### 9.1 Priorytety implementacji

#### Faza 1 (MVP Auth - 2 tygodnie)
1. **Backend rozszerzenia**:
   - Register endpoint z Supabase
   - Forgot/Reset password endpoints
   - Email verification
   - Basic household management

2. **Frontend foundation**:
   - Astro project setup
   - Basic auth pages (login, register, forgot password)
   - Auth context i guards
   - Basic styling i UX

#### Faza 2 (Full Auth Flow - 1 tydzień)
1. **Advanced features**:
   - Complete onboarding wizard
   - Household member management
   - Role-based access control
   - Advanced error handling

2. **Polish i Security**:
   - Complete form validation
   - Security features (rate limiting, CSRF)
   - Email templates
   - Responsive design

#### Faza 3 (Production Ready - 1 tydzień)
1. **Testing i Quality**:
   - Unit tests
   - Integration tests
   - E2E tests
   - Performance optimization

2. **Deployment i Monitoring**:
   - CI/CD pipeline
   - Production configuration
   - Monitoring i analytics
   - Documentation

### 9.2 Kluczowe zależności

#### 9.2.1 Techniczne
- Konfiguracja Supabase Auth settings
- Setup email provider (Supabase/SendGrid)
- Frontend build pipeline
- Database migrations deployment

#### 9.2.2 Biznesowe
- Final UX/UI designs
- Email templates content
- Terms of Service i Privacy Policy
- RODO compliance review

### 9.3 Ryzyka i mitigacje

#### 9.3.1 Techniczne ryzyka
- **Supabase Auth limitations**: Backup plan z custom JWT
- **Email deliverability**: Multiple provider setup
- **Performance przy skali**: Caching strategy
- **Security vulnerabilities**: Regular security audits

#### 9.3.2 UX ryzyka  
- **Kompleksowy onboarding**: A/B testing różnych flow
- **Mobile experience**: Progressive Web App approach
- **Accessibility**: WCAG 2.1 compliance testing

---

**Dokument opracowany**: 18 października 2025  
**Wersja**: 1.0  
**Status**: Specyfikacja techniczna do implementacji
