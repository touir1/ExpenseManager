# Use the official .NET 8 SDK image to build the app
ARG REGISTRY=mcr.microsoft.com
FROM ${REGISTRY}/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Touir.ExpensesManager.Notifications/Touir.ExpensesManager.Notifications.csproj Touir.ExpensesManager.Notifications/
COPY Touir.ExpensesManager.Notifications.Tests/Touir.ExpensesManager.Notifications.Tests.csproj Touir.ExpensesManager.Notifications.Tests/

# Restore using the solution file
RUN dotnet restore ./Touir.ExpensesManager.Notifications.sln

# Copy the rest of the source code
COPY . .

# Build and publish the main project
WORKDIR /src/Touir.ExpensesManager.Notifications
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the chiseled (minimal, non-root) ASP.NET Core runtime image
FROM ${REGISTRY}/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Touir.ExpensesManager.Notifications.dll"]
