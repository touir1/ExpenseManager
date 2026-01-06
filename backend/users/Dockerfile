# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Touir.ExpensesManager.Users/Touir.ExpensesManager.Users.csproj Touir.ExpensesManager.Users/
COPY Touir.ExpensesManager.Users.Tests/Touir.ExpensesManager.Users.Tests.csproj Touir.ExpensesManager.Users.Tests/

# Restore using the solution file
RUN dotnet restore ./Touir.ExpensesManager.Users.sln

# Copy the rest of the source code
COPY . .

# Build and publish the main project
WORKDIR /src/Touir.ExpensesManager.Users
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "Touir.ExpensesManager.Users.dll"]