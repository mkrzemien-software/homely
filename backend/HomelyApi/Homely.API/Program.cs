using Homely.API.Configuration;
using Homely.API.Extensions;
using Homely.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Supabase;
using System.Net;
using System.Text;
using Client = Supabase.Client;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ENVIRONMENT SETUP
// ============================================================================
// ASPNETCORE_ENVIRONMENT can be: Development, Production, Local, Dev
var environmentName = builder.Environment.EnvironmentName;
Console.WriteLine($"🌍 Starting Homely API in {environmentName} environment");

// Load environment-specific configuration
// Priority: appsettings.{Environment}.json > Environment Variables > AWS Systems Manager
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()  // Override with environment variables
    .AddCommandLine(args);

// ============================================================================
// CONFIGURATION SECTIONS
// ============================================================================
builder.Services.Configure<EnvironmentSettings>(
    builder.Configuration.GetSection(EnvironmentSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(SupabaseSettings.SectionName));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Get configuration values
var environmentSettings = builder.Configuration.GetSection(EnvironmentSettings.SectionName).Get<EnvironmentSettings>()
    ?? new EnvironmentSettings { Name = environmentName };
var supabaseSettings = builder.Configuration.GetSection(SupabaseSettings.SectionName).Get<SupabaseSettings>()
    ?? throw new InvalidOperationException("Supabase configuration is missing");
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

Console.WriteLine($"📊 Environment: {environmentSettings.Name} - {environmentSettings.Description}");

// ============================================================================
// DATABASE CONFIGURATION
// ============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Mask sensitive data in connection string for logging
var maskedConnectionString = MaskConnectionString(connectionString);
Console.WriteLine($"🔗 Database Connection: {maskedConnectionString}");

// Add Homely services (Entity Framework + Repositories)
builder.Services.AddHomelyServices(connectionString);

// ============================================================================
// SUPABASE CLIENT (for authentication)
// ============================================================================
builder.Services.AddSingleton(provider =>
{
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false // We don't need realtime for authentication
    };
    
    var client = new Client(supabaseSettings.Url, supabaseSettings.Key, options);
    
    // Initialize the client
    return client;
});

// ============================================================================
// HTTP CLIENT FACTORY (for Supabase Admin API)
// ============================================================================
builder.Services.AddHttpClient();

// ============================================================================
// BUSINESS SERVICES
// ============================================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();

// ============================================================================
// CORE SERVICES
// ============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ensure camelCase property naming for JSON serialization
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ============================================================================
// AUTHENTICATION & AUTHORIZATION
// ============================================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.ValidIssuer,
        ValidAudience = jwtSettings.ValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero // Remove delay of token when expires
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated for user");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================================================
// API DOCUMENTATION (Swagger)
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Homely API",
        Version = "v1",
        Description = "API for Homely - Home Management System with Repository Pattern",
        Contact = new OpenApiContact
        {
            Name = "Homely Team",
            Email = "support@homely.app"
        }
    });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ============================================================================
// CORS POLICY
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Local")
        {
            // Development and Local environment origins
            policy.WithOrigins(
                      "https://dev.homely.maflint.com",
                      "http://localhost:4200"  // Alternative local port
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Production environment origins
            policy.WithOrigins(
                      "https://homely.maflint.com",
                      "https://maflint.com"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// ============================================================================
// CONFIGURE HTTP REQUEST PIPELINE
// ============================================================================

// Configure ForwardedHeaders for ALB/CloudFront
// This middleware must be placed BEFORE UseHttpsRedirection and UseAuthentication
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Trust ALB/CloudFront proxy
    KnownNetworks = { new IPNetwork(IPAddress.Parse("0.0.0.0"), 0) },
    KnownProxies = { }
});

if (app.Environment.IsDevelopment() ||
    builder.Environment.EnvironmentName == "Local" ||
    builder.Environment.EnvironmentName == "E2E")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Homely API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Homely API Documentation - Repository Pattern";
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    if (!(app.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Local"))
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================================
// HEALTH CHECK ENDPOINT
// ============================================================================
app.MapGet("/health", () => new { 
        Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Environment = environmentName,
        Database = "EntityFramework with Repository Pattern",
        Authentication = "Supabase + JWT"
    })
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();

// ============================================================================
// HELPER METHODS
// ============================================================================
static string MaskConnectionString(string connectionString)
{
    var parts = connectionString.Split(';');
    var maskedParts = parts.Select(part =>
    {
        if (part.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            var index = part.IndexOf('=') + 1;
            return part.Substring(0, index) + "****";
        }
        return part;
    });
    return string.Join(";", maskedParts);
}

// Make Program class accessible for integration tests (WebApplicationFactory)
public partial class Program { }
