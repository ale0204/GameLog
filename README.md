# GameForum

Backend API pentru o platformă de comunitate gaming care combină un forum cu un
tracker de game-backlog. Minimum Viable Product fără frontend (vine ulterior cu React +
Tailwind), fără autentificare reală (placeholder-uri arhitecturale).

## Stack

- ASP.NET Core .NET 9, Clean Architecture
- EF Core + Npgsql + PostgreSQL
- Serilog (prin abstracția `IAppLogger`)
- xUnit + Moq pentru unit tests, EF InMemory pentru integration tests
- RAWG.io API pentru date jocuri
- Python (`requests`, `psycopg2`, `faker`) pentru scripturi de seed

## Prerequisites

- .NET 9 SDK
- PostgreSQL 16 instalat local pe portul 5432
- (Opțional) Docker Desktop pentru rularea Postgres-ului în container la stagiul final
- (Opțional) Python 3.11+ pentru scripturile de seed

## Structura soluției

```
GameForum/
├── GameForum.Presentation/    Web API + controllere + Program.cs
├── GameForum.Application/     handler-e per use case + abstracții
├── GameForum.DataAccess/      EF Core context + repositories + UnitOfWork
├── GameForum.Infrastructure/  Serilog, RawgClient, BackgroundServices
├── GameForum.Domain/          entități, enum-uri, excepții
├── GameForum.Tests.Unit/      xUnit + Moq
└── GameForum.Tests.Integration/  WebApplicationFactory + EF InMemory
```

Dependency flow: `Presentation → Application → Domain` (DataAccess și Infrastructure
intră în Application).

## Rulare locală

```bash
# Restore + build
dotnet restore
dotnet build

# Aplică migrările pe Postgres-ul local
# (Stage 4+ — connection string în appsettings.Development.json)
dotnet ef database update -p GameForum.DataAccess -s GameForum.Presentation

# Rulează API-ul
dotnet run --project GameForum.Presentation
```

Swagger UI la `https://localhost:<port>/swagger` (Development).

## Teste

```bash
# Toate testele
dotnet test

# Doar unit tests
dotnet test GameForum.Tests.Unit

# Doar integration tests
dotnet test GameForum.Tests.Integration
```

## Configurare

`appsettings.Development.json` (gitignored) conține:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=gameforum;Username=...;Password=..."
  },
  "RAWG": {
    "ApiKey": "..."
  }
}
```
