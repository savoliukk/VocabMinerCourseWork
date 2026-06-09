# VocabMiner CourseWork API

VocabMiner CourseWork API is an ASP.NET Core 8 Web API project for the course
`Алгоритмізація та програмування`. The system stores learning content, splits it
into segments, saves vocabulary items with context, schedules simple reviews,
and exports cards to CSV or TSV.

## Stack

- ASP.NET Core 8 Web API
- PostgreSQL
- Entity Framework Core
- Repository and service architecture
- Swagger for API inspection
- xUnit tests
- Docker Compose deployment with Caddy HTTPS proxy

## Project Structure

```text
VocabMinerCourseWork/
  Controllers/
  Business Logic/
  Repositories/
  Domains/
    Entities/
    ViewModels/
  Data/
  Migrations/
  docs/
VocabMinerCourseWork.Tests/
```

## Run with Docker Compose

Docker Desktop must be running before this command:

```powershell
Copy-Item .\VocabMinerCourseWork\.env.example .\VocabMinerCourseWork\.env
docker compose -f .\VocabMinerCourseWork\docker-compose.yml up -d --build
```

The Compose stack runs:

- `api` - ASP.NET Core API on the internal Docker network
- `postgres` - PostgreSQL with a named volume
- `caddy` - reverse proxy on ports `80` and `443`

For local smoke testing through Caddy:

```powershell
Invoke-RestMethod http://localhost/
```

For the public coursework demo, point `vocabminer.savoliukk.pp.ua` to the VM
public IP and open:

```text
https://vocabminer.savoliukk.pp.ua/swagger
```

Deployment details:

- `VocabMinerCourseWork/docs/deploy-azure-vps.md` - Azure Free Services / Azure for Students
- `VocabMinerCourseWork/docs/deploy-oracle-vps.md` - Oracle Cloud Always Free fallback

Default local development connection string:

```text
Host=localhost;Port=5432;Database=vocabminer_coursework;Username=vocabminer;Password=vocabminer123
```

These credentials are for the local coursework Docker database only.

## Restore, Build, and Test

```powershell
$env:DOTNET_CLI_HOME = (Join-Path (Get-Location).Path '.dotnet-home')
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
dotnet restore .\VocabMinerCourseWork.sln --configfile .\NuGet.Config
dotnet build .\VocabMinerCourseWork.sln --no-restore
dotnet test .\VocabMinerCourseWork.sln --no-restore -p:UseAppHost=false
```

## Run the API

```powershell
$env:ApplyMigrations = 'true'
dotnet run --project .\VocabMinerCourseWork\VocabMinerCourseWork.Api.csproj
```

Swagger is available in development at:

```text
https://localhost:<port>/swagger
```

The seed user is:

```text
email: student@example.com
password: Password123!
id: 11111111-1111-1111-1111-111111111111
```

## Main Endpoints

The API contains controllers for authentication, content sources, text segments,
learning units, reviews, and exports.

- `POST /auth/register`
- `POST /auth/login`
- `GET /auth/profile/{id}`
- `GET/POST/PUT/DELETE /content-sources`
- `GET/PUT/DELETE /segments`
- `GET/POST/PUT/DELETE /learning-units`
- `POST /learning-units/{id}/explain`
- `POST /learning-units/promote-phrase`
- `GET /reviews/today/{userId}`
- `POST /reviews/submit`
- `POST /reviews/reset/{learningUnitId}`
- `POST /exports`
- `GET /exports/{id}/download`
