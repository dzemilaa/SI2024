# Koristi .NET SDK sliku za build fazu
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiraj csproj i restore zavisnosti
COPY ["backend.csproj", ""]
RUN dotnet restore "backend.csproj"

# Kopiraj sve fajlove
COPY . .

# Build aplikacije
RUN dotnet build "backend.csproj" -c Release -o /app/build

# Publish aplikacije
RUN dotnet publish "backend.csproj" -c Release -o /app/publish

# Koristi .NET ASP.NET sliku za finalnu fazu
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kopiraj objavljen fajl iz build faze
COPY --from=build /app/publish .

# Otvori port 80
EXPOSE 80

# Pokreni aplikaciju
ENTRYPOINT ["dotnet", "backend.dll"]
