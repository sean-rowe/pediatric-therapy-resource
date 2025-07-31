using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UPTRMS.Api.Data;
using UPTRMS.Api.Models.Domain;
using UPTRMS.Api.Services;
using UPTRMS.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        policy =>
        {
            policy.WithOrigins(
                    builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Password hasher configuration (using custom implementation without Identity)
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 10000;
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    // Add role-based policies
    options.AddPolicy("TherapistOnly", policy => policy.RequireRole("Therapist", "Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOnly", policy => policy.RequireClaim("is_seller", "true"));
    options.AddPolicy("OrganizationAdmin", policy => policy.RequireRole("OrganizationAdmin", "Admin"));

    // Add subscription-based policies
    options.AddPolicy("ProSubscription", policy =>
        policy.RequireClaim("subscription_tier", "Pro", "SmallGroup", "LargeGroup", "Enterprise"));
    options.AddPolicy("GroupSubscription", policy =>
        policy.RequireClaim("subscription_tier", "SmallGroup", "LargeGroup", "Enterprise"));
});

// Register application services
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Check configuration to determine which repository implementation to use
bool useStoredProcedures = builder.Configuration.GetValue<bool>("UseStoredProcedures", true);

if (useStoredProcedures)
{
    // Use stored procedure implementations
    builder.Services.AddScoped<IUserRepository, UserRepositoryStoredProc>();
    builder.Services.AddScoped<IResourceRepository, ResourceRepositoryStoredProc>();
    builder.Services.AddScoped<IStudentRepository, StudentRepositoryStoredProc>();
    builder.Services.AddScoped<ISessionRepository, SessionRepositoryStoredProc>();
    builder.Services.AddScoped<IMarketplaceRepository, MarketplaceRepositoryStoredProc>();
}
else
{
    // Use Entity Framework implementations (for backward compatibility)
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
    // Note: Student, Session, and Marketplace repositories don't have EF implementations yet
    throw new NotSupportedException("Entity Framework implementations for Student, Session, and Marketplace repositories are not available. Set UseStoredProcedures to true.");
}

// Register HttpContextAccessor for audit service
builder.Services.AddHttpContextAccessor();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UPTRMS API",
        Version = "v1",
        Description = "Unified Pediatric Therapy Resource Management System API"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (!builder.Environment.IsDevelopment())
{
    // Add production logging providers (e.g., Application Insights, Serilog, etc.)
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UPTRMS API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}
else
{
    // Production error handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

// Error handling endpoint
app.Map("/error", () => Results.Problem("An error occurred processing your request"))
    .AllowAnonymous();

// Apply database migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }