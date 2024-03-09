using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Infrastructure.Contracts;
using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Repositories;
using com.touir.expenses.Users.Repositories.Contracts;
using com.touir.expenses.Users.Services;
using com.touir.expenses.Users.Services.Contracts;

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

builder.Services.Configure<JwtAuthOptions>(c =>
{
    c.SecretKey = builder.Configuration.GetValue("JwtAuth:SecretKey",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_JWT_SECRET_KEY")) ?? "SECRET_KEY_TO_CHANGE_LATER";
    c.ExpiryInMinutes = int.Parse(builder.Configuration.GetValue("JwtAuth:ExpiryInMinutes",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_JWT_EXPIRY_IN_MINUTES")) ?? "60");
    c.Audience = builder.Configuration.GetValue("JwtAuth:Audience",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_JWT_AUDIENCE")) ?? "https://localhost";
    c.Issuer = builder.Configuration.GetValue("JwtAuth:Issuer",
                    Environment.GetEnvironmentVariable("EXPENSE_MANAGEMENT_USERS_JWT_ISSUER")) ?? "https://localhost";
});

builder.Services.Configure<EmailOptions>(c =>
{
    c.Email = builder.Configuration.GetValue("EmailAuth:Email",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_EMAILAUTH_EMAIL")) ?? "email.to.change.later@email.com";
    c.Password = builder.Configuration.GetValue("EmailAuth:Password",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PASSWORD")) ?? "PASSWORD_TO_CHANGE_LATER";
    c.Host = builder.Configuration.GetValue("EmailAuth:Host",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_EMAILAUTH_HOST")) ?? "smtp.gmail.com";
    c.Port = int.Parse(builder.Configuration.GetValue("EmailAuth:Port",
                    Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_EMAILAUTH_PORT")) ?? "587");
});

#endregion

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

#endregion

#region Services

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRoleService, RoleService>();

#endregion

#region Helpers

builder.Services.AddScoped<IEmailHelper, EmailHelper>();

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
