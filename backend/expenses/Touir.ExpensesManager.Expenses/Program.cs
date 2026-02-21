using Touir.ExpensesManager.Expenses.Repositories.External;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

#region Options

builder.Services.Configure<RabbitMQOptions>(c =>
{
    c.HostName = builder.Configuration.GetValue("RabbitMQ:HostName",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_RABBITMQ_HOSTNAME") ?? "127.0.0.1");
    c.Port = int.Parse(builder.Configuration.GetValue("RabbitMQ:Port",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_RABBITMQ_PORT")) ?? "5672");
    c.UserName = builder.Configuration.GetValue("RabbitMQ:UserName",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_RABBITMQ_USERNAME") ?? "expense_expenses");
    c.Password = builder.Configuration.GetValue("RabbitMQ:Password",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_RABBITMQ_PASSWORD") ?? "expense_expenses");
});

builder.Services.Configure<PostgresOptions>(c =>
{
    c.Server = builder.Configuration.GetValue("Postgres:Server",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_DATABASE_SERVER")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("Postgres:Port",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_DATABASE_PORT")) ?? "5432");
    c.UserName = builder.Configuration.GetValue("Postgres:User",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_DATABASE_USERNAME")) ?? "expenses";
    c.Password = builder.Configuration.GetValue("Postgres:Password",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_DATABASE_PASSWORD")) ?? "expenses";
    c.Database = builder.Configuration.GetValue("Postgres:Database",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_EXPENSES_DATABASE_DATABASE")) ?? "expenses";
});

#endregion

#region Services

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

#endregion

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

#region Database

builder.Services.AddDbContext<ExpensesDbContext>((sp, options) =>
{
    var pgOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
    options.UseNpgsql(pgOptions.ConnectionString);
});

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

var app = builder.Build();

// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpensesDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
