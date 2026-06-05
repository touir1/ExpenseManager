using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Messaging;
using Touir.ExpensesManager.Users.Messaging.Publishers;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Services.Contracts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "MISSING_PARAMETERS";
        return new UnauthorizedObjectResult(new ErrorResponse { Message = firstError });
    };
});

builder.Services.AddEndpointsApiExplorer();

#region Swagger

//Swagger Documentation Section
var info = new OpenApiInfo()
{
    Title = "User service API documentation",
    Version = "v1",
    Description = "User management system",
    Contact = new OpenApiContact()
    {
        Name = "Mohamed Ali Touir",
        Email = "touir.mat@gmail.com",
    }
};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info);

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token from POST /auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region Options

builder.Services.Configure<PostgresOptions>(c =>
{
    c.Server = builder.Configuration.GetValue("Postgres:Server",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_DATABASE_SERVER")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("Postgres:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_DATABASE_PORT")) ?? "5432");
    c.UserName = builder.Configuration.GetValue("Postgres:User",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_DATABASE_USERNAME")) ?? "users";
    c.Password = builder.Configuration.GetValue("Postgres:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_DATABASE_PASSWORD")) ?? "users";
    c.Database = builder.Configuration.GetValue("Postgres:Database",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_DATABASE_DATABASE")) ?? "users";
});

builder.Services.Configure<JwtAuthOptions>(c =>
{
    c.SecretKey = builder.Configuration.GetValue("JwtAuth:SecretKey",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_JWT_SECRET_KEY")) ?? "SECRET_KEY_TO_CHANGE_LATER";
    c.ExpiryInMinutes = int.Parse(builder.Configuration.GetValue("JwtAuth:ExpiryInMinutes",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_JWT_EXPIRY_IN_MINUTES")) ?? "60");
    c.Audience = builder.Configuration.GetValue("JwtAuth:Audience",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_JWT_AUDIENCE")) ?? "https://localhost";
    c.Issuer = builder.Configuration.GetValue("JwtAuth:Issuer",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_JWT_ISSUER")) ?? "https://localhost";
    c.ShortLivedRefreshExpiryInDays = int.Parse(builder.Configuration.GetValue("JwtAuth:ShortLivedRefreshExpiryInDays",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_JWT_SHORT_REFRESH_EXPIRY_IN_DAYS")) ?? "1");
});

builder.Services.Configure<AuthenticationServiceOptions>(c =>
{
    c.VerifyEmailBaseUrl = builder.Configuration.GetValue("AuthenticationService:VerifyEmailBaseUrl",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL")) ?? "https://localhost:7114/api/auth/verifyEmail";
    c.ResetPasswordBaseUrl = builder.Configuration.GetValue("AuthenticationService:ResetPasswordBaseUrl",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_RESET_PASSWORD_URL")) ?? "https://localhost/reset-password";
    c.EmailVerificationExpiryInHours = int.Parse(builder.Configuration.GetValue("AuthenticationService:EmailVerificationExpiryInHours",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_EMAIL_VERIFICATION_EXPIRY_IN_HOURS")) ?? "24");
    c.PasswordResetExpiryInHours = int.Parse(builder.Configuration.GetValue("AuthenticationService:PasswordResetExpiryInHours",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_PASSWORD_RESET_EXPIRY_IN_HOURS")) ?? "24");
});

builder.Services.Configure<CryptographyOptions>(c =>
{
    c.MaximumSaltSize = int.Parse(builder.Configuration.GetValue("Cryptography:MaximumSaltSize",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_CRYPTOGRAPHY_MAXIMUM_SALT_SIZE")) ?? "32");
});

builder.Services.Configure<RabbitMQOptions>(c =>
{
    c.HostName = builder.Configuration.GetValue("RabbitMQ:HostName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_RABBITMQ_HOSTNAME")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("RabbitMQ:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_RABBITMQ_PORT")) ?? "5672");
    c.UserName = builder.Configuration.GetValue("RabbitMQ:UserName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_RABBITMQ_USERNAME")) ?? "EXPENSES_users";
    c.Password = builder.Configuration.GetValue("RabbitMQ:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_RABBITMQ_PASSWORD")) ?? "EXPENSES_users";
    c.VirtualHost = builder.Configuration.GetValue("RabbitMQ:VirtualHost",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_RABBITMQ_VIRTUALHOST")) ?? "expense_management";
});

#endregion

#region DbContext

builder.Services.AddDbContext<UsersAppDbContext>((serviceProvider, options) =>
{
    var pgOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;
    options.UseNpgsql(pgOptions.ConnectionString);
});

#endregion

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

#endregion

#region Messaging

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<IUserEventPublisher, UserEventPublisher>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddHostedService<OutboxPublisherService>();

#endregion

#region Services

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPasswordManagementService, PasswordManagementService>();
builder.Services.AddScoped<IUserRoleAssignmentService, UserRoleAssignmentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

#endregion

#region Helpers

builder.Services.AddScoped<ICryptographyHelper, CryptographyHelper>();

#endregion

#region Cors Policy

// Add CORS policy to allow requests from localhost on any port
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
            "http://localhost",
            "https://localhost"
        )
        .SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost:") ||
            origin.StartsWith("https://localhost:")
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

#endregion

#region Health Checks

string[] ReadyDb = { "ready", "db" };

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<UsersAppDbContext>("database", tags: ReadyDb);

#endregion

#region Rate Limiting

static string GetClientIp(HttpContext ctx) =>
    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    void AddFixedWindow(string name, int permitLimit, int windowSeconds) =>
        options.AddPolicy(name, ctx => RateLimitPartition.GetFixedWindowLimiter(
            GetClientIp(ctx),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    AddFixedWindow("login",                 10,  60);
    AddFixedWindow("register",               5, 600);
    AddFixedWindow("resend_verification",    3, 600);
    AddFixedWindow("validate_email",        10, 300);
    AddFixedWindow("request_password_reset", 5, 600);
    AddFixedWindow("change_password_reset",  5, 600);
    AddFixedWindow("refresh",               20,  60);
    AddFixedWindow("change_password",       10, 300);
    AddFixedWindow("create_password",        5, 600);
    AddFixedWindow("messaging_replay",       5,  60);
});

#endregion

var app = builder.Build();

// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersAppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");

    // Add /api/users prefix for all routes in development
    app.UsePathBase("/api/users");
}

if(app.Environment.IsDevelopment() || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

await app.RunAsync();
