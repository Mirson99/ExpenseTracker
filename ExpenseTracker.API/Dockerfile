# ETAP 1: Budowanie (SDK w wersji 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR .

# Kopiowanie i restore
COPY ["ExpenseTracker.API/ExpenseTracker.API.csproj", "ExpenseTracker.API/"]
COPY ["ExpenseTracker.Application/ExpenseTracker.Application.csproj", "ExpenseTracker.Application/"]
COPY ["ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj", "ExpenseTracker.Infrastructure/"]
COPY ["ExpenseTracker.Domain/ExpenseTracker.Domain.csproj", "ExpenseTracker.Domain/"]

# Restore dla głównego projektu
RUN dotnet restore "ExpenseTracker.API/ExpenseTracker.API.csproj"

# Kopiowanie reszty kodu
COPY . .
# Budowanie (Publish)
WORKDIR "ExpenseTracker.API"
RUN dotnet publish "ExpenseTracker.API.csproj" -c Release -o /app/publish

# ETAP 2: Uruchamianie (Runtime w wersji 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseTracker.API.dll"]