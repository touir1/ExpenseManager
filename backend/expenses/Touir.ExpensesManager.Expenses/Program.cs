using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using Touir.ExpensesManager.Expenses.Messaging.Consumers;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Repositories.External;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.Extensions.Caching.Memory;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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
    Title = "Expenses service API documentation",
    Version = "v1",
    Description = "Expense management system",
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
    {
        c.IncludeXmlComments(xmlPath);
    }
});
#endregion

#region Options
builder.Services.Configure<RabbitMQOptions>(c =>
{
    c.HostName = builder.Configuration.GetValue("RabbitMQ:HostName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_HOSTNAME")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("RabbitMQ:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PORT")) ?? "5672");
    c.UserName = builder.Configuration.GetValue("RabbitMQ:UserName",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_USERNAME")) ?? "expense_expenses";
    c.Password = builder.Configuration.GetValue("RabbitMQ:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_PASSWORD")) ?? "expense_expenses";
    c.VirtualHost = builder.Configuration.GetValue("RabbitMQ:VirtualHost",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_RABBITMQ_VIRTUALHOST")) ?? "expense_management";
});

builder.Services.Configure<PostgresOptions>(c =>
{
    c.Server = builder.Configuration.GetValue("Postgres:Server",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_DATABASE_SERVER")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("Postgres:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_DATABASE_PORT")) ?? "5432");
    c.UserName = builder.Configuration.GetValue("Postgres:User",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_DATABASE_USERNAME")) ?? "expenses";
    c.Password = builder.Configuration.GetValue("Postgres:Password",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_DATABASE_PASSWORD")) ?? "expenses";
    c.Database = builder.Configuration.GetValue("Postgres:Database",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_EXPENSES_DATABASE_DATABASE")) ?? "expenses";
});
#endregion

#region Services
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<ILookupCacheService, LookupCacheService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IExpenseAuditService, ExpenseAuditService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
#endregion

#region Messaging
builder.Services.AddHostedService<UserEventConsumer>();
#endregion

#region Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
#endregion

#region Database
builder.Services.AddDbContext<ExpensesDbContext>((sp, options) =>
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
            .AllowAnyMethod();
    });
});
#endregion

#region Health Checks
string[] ReadyDb = { "ready", "db" };
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ExpensesDbContext>("database", tags: ReadyDb);
#endregion

#region Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("expenses_global", ctx => RateLimitPartition.GetSlidingWindowLimiter(
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
    var db = scope.ServiceProvider.GetRequiredService<ExpensesDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
    app.UsePathBase("/api/expenses");
}

if (app.Environment.IsDevelopment() || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
await app.RunAsync();
