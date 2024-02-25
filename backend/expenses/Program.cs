using com.touir.expenses.Expenses.Repositories;
using com.touir.expenses.Expenses.Services;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
