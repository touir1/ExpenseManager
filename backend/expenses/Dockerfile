# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Touir.ExpensesManager.Expenses/Touir.ExpensesManager.Expenses.csproj Touir.ExpensesManager.Expenses/
COPY Touir.ExpensesManager.Expenses.Tests/Touir.ExpensesManager.Expenses.Tests.csproj Touir.ExpensesManager.Expenses.Tests/

# Restore using the solution file
RUN dotnet restore ./Touir.ExpensesManager.Expenses.sln

# Copy the rest of the source code
COPY . .

# Build and publish the main project
WORKDIR /src/Touir.ExpensesManager.Expenses
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Touir.ExpensesManager.Expenses.dll"]