using com.touir.expenses.Users.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Options

builder.Services.Configure<PostgresOptions>(c =>
{
    c.Server = builder.Configuration.GetValue("Postgres:Server",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_DATABASE_SERVER")) ?? "127.0.0.1";
    c.Port = int.Parse(builder.Configuration.GetValue("Postgres:Port",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_DATABASE_PORT")) ?? "5432");
    c.UserName = builder.Configuration.GetValue("Postgres:User",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_DATABASE_USERNAME")) ?? "users";
    c.Password = builder.Configuration.GetValue("Postgres:Password",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_DATABASE_PASSWORD")) ?? "users";
    c.Database = builder.Configuration.GetValue("Postgres:Database",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_DATABASE_DATABASE")) ?? "users";
});

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
