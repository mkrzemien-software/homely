using Homely.API.Configuration;
using Homely.API.Extensions;
using Homely.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Supabase;
using System.Text;
using Client = Supabase.Client;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURATION SECTIONS
// ============================================================================
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(SupabaseSettings.SectionName));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Get configuration values
var supabaseSettings = builder.Configuration.GetSection(SupabaseSettings.SectionName).Get<SupabaseSettings>()
    ?? throw new InvalidOperationException("Supabase configuration is missing");
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

// ============================================================================
// DATABASE CONFIGURATION
// ============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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
        if (builder.Environment.IsDevelopment())
        {
            // Allow any origin in development
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Restrict origins in production
            policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
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
if (app.Environment.IsDevelopment())
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
    
    if (!app.Environment.IsDevelopment())
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
        Database = "EntityFramework with Repository Pattern",
        Authentication = "Supabase + JWT"
    })
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
