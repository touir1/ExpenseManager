using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Infrastructure.Contracts;
using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Repositories;
using com.touir.expenses.Users.Repositories.Contracts;
using com.touir.expenses.Users.Services;
using com.touir.expenses.Users.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
});

#endregion

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

builder.Services.Configure<AuthenticationServiceOptions>(c =>
{
    c.VerifyEmailBaseUrl = builder.Configuration.GetValue("AuthenticationService:VerifyEmailBaseUrl",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_USERS_AUTHSERVICE_VERIFY_EMAIL_URL")) ?? "https://localhost:7114/api/auth/verifyEmail";
});

builder.Services.Configure<CryptographyOptions>(c =>
{
    c.MaximumSaltSize = int.Parse(builder.Configuration.GetValue("Cryptography:MaximumSaltSize",
                Environment.GetEnvironmentVariable("EXPENSES_MANAGEMENT_CRYPTOGRAPHY_MAXIMUM_SALT_SIZE")) ?? "32");
});

#endregion

#region DbContext

builder.Services.AddDbContext<UsersAppDbContext>((serviceProvider, options) =>
{
    var pgOptions = serviceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value;
    var connStr = $"Host={pgOptions.Server};Port={pgOptions.Port};Database={pgOptions.Database};Username={pgOptions.UserName};Password={pgOptions.Password}";
    options.UseNpgsql(connStr);
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
builder.Services.AddScoped<ICryptographyHelper, CryptographyHelper>();

#endregion

var app = builder.Build();

// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersAppDbContext>();
    db.Database.Migrate();
}

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
