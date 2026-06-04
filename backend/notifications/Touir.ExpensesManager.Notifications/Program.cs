using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Touir.ExpensesManager.Notifications.Controllers.Responses;
using Touir.ExpensesManager.Notifications.Hubs;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Infrastructure.Contracts;
using Touir.ExpensesManager.Notifications.Infrastructure.Options;
using Touir.ExpensesManager.Notifications.Messaging.Consumers;
using Touir.ExpensesManager.Notifications.Repositories;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;
using Touir.ExpensesManager.Notifications.Services;
using Touir.ExpensesManager.Notifications.Services.Contracts;

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
var info = new OpenApiInfo()
{
    Title = "Notifications service API documentation",
    Version = "v1",
    Description = "Expense management notifications",
    Contact = new OpenApiContact()
    {
        Name = "Mohamed Ali Touir",
        Email = "touir.mat@gmail.com",
    }
};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info);
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
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
builder.Services.Configure<RabbitMQOptions>(c =>
{
    c.HostName = builder.Configuration.GetValue("RabbitMQ:HostName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_HOSTNAME")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("RabbitMQ:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_PORT")) ?? "5672");
    c.UserName = builder.Configuration.GetValue("RabbitMQ:UserName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_USERNAME")) ?? "expense_notifications";
    c.Password = builder.Configuration.GetValue("RabbitMQ:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_PASSWORD")) ?? "expense_notifications";
    c.VirtualHost = builder.Configuration.GetValue("RabbitMQ:VirtualHost",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_RABBITMQ_VIRTUALHOST")) ?? "expense_management";
});

builder.Services.Configure<PostgresOptions>(c =>
{
    c.Server = builder.Configuration.GetValue("Postgres:Server",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_SERVER")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("Postgres:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_PORT")) ?? "5432");
    c.UserName = builder.Configuration.GetValue("Postgres:User",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_USERNAME")) ?? "notifications";
    c.Password = builder.Configuration.GetValue("Postgres:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_PASSWORD")) ?? "notifications";
    c.Database = builder.Configuration.GetValue("Postgres:Database",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_DATABASE_DATABASE")) ?? "notifications";
});

builder.Services.Configure<EmailOptions>(c =>
{
    c.Email = builder.Configuration.GetValue("EmailAuth:Email",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_EMAIL")) ?? "email.to.change.later@email.com";
    c.Password = builder.Configuration.GetValue("EmailAuth:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_PASSWORD")) ?? string.Empty;
    c.Host = builder.Configuration.GetValue("EmailAuth:Host",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_HOST")) ?? "smtp.gmail.com";
    c.Port = int.Parse(builder.Configuration.GetValue("EmailAuth:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_PORT")) ?? "587");
    c.EnableSsl = bool.Parse(builder.Configuration.GetValue("EmailAuth:EnableSsl",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_NOTIFICATIONS_EMAILAUTH_ENABLE_SSL")) ?? "true");
});
#endregion

#region Services
builder.Services.AddSignalR();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
#endregion

#region Messaging
builder.Services.AddHostedService<FamilyEventConsumer>();
#endregion

#region Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
#endregion

#region Database
builder.Services.AddDbContext<NotificationsDbContext>((sp, options) =>
{
    var pgOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
    options.UseNpgsql(pgOptions.ConnectionString);
});
#endregion

#region Cors Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost", "https://localhost")
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost:") ||
                origin.StartsWith("https://localhost:")
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
#endregion

#region Health Checks
string[] ReadyDb = { "ready", "db" };
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<NotificationsDbContext>("database", tags: ReadyDb);
#endregion

#region Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("notifications_global", ctx => RateLimitPartition.GetSlidingWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromSeconds(60),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        }));
});
#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
    app.UsePathBase("/api/notifications");
}

if (app.Environment.IsDevelopment() || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/ws/notifications");
app.MapHealthChecks("/health");
await app.RunAsync();
