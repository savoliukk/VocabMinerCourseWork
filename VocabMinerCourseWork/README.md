# VocabMiner CourseWork API

VocabMiner CourseWork API is an ASP.NET Core 8 server project for the course
`Алгоритмізація та програмування`. The system stores learning content, splits it
into segments, saves vocabulary items with context, schedules simple reviews,
and exports cards to CSV/TSV.

## Stack

- ASP.NET Core 8 Web API
- PostgreSQL
- Entity Framework Core
- Repository + Service architecture
- Swagger for API inspection

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
```

## Run PostgreSQL

Docker Desktop must be running before this command:

```powershell
cd VocabMinerCourseWork
docker compose up -d
```

Default connection string:

```text
Host=localhost;Port=5432;Database=vocabminer_coursework;Username=vocabminer;Password=vocabminer123
```

## Restore, Build, and Run

```powershell
$env:DOTNET_CLI_HOME = (Join-Path (Get-Location).Path '..\.dotnet-home')
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
dotnet restore .\VocabMinerCourseWork.Api.csproj --configfile ..\NuGet.Config
dotnet restore ..\VocabMinerCourseWork.Tests\VocabMinerCourseWork.Tests.csproj --configfile ..\NuGet.Config
dotnet build .\VocabMinerCourseWork.Api.csproj
dotnet build ..\VocabMinerCourseWork.Tests\VocabMinerCourseWork.Tests.csproj
$env:ApplyMigrations = 'true'
dotnet run --project .\VocabMinerCourseWork.Api.csproj
```

Swagger opens at:

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

The API contains 105 endpoints. Each of the six controllers has at least 15
endpoints, including CRUD, filtering, summary, preview, validation, and quick
action routes.

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

See `VocabMinerCourseWork.Api.http` for a complete manual scenario.

## Tests

```powershell
dotnet test ..\VocabMinerCourseWork.Tests\VocabMinerCourseWork.Tests.csproj
```

The tests cover segmentation, learning-unit normalization, review scheduling,
and CSV/TSV export generation.
