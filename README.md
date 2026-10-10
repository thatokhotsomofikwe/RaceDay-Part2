# RaceDay — Part 2 API

RaceDay is a South African road running, walking and cycling event management system. Part 2 implements the ASP.NET Core Web API that supports account registration and login, profile management, events, categories, enrolments, results, route information and event-day weather.

> **Technology:** C#, ASP.NET Core Web API, .NET 8, Entity Framework Core, SQL Server, Swagger/OpenAPI, xUnit and GitHub Actions.

## Main features

- Registration for the `Organiser` and `Participant` roles.
- Passwords hashed with ASP.NET Core's `PasswordHasher<TUser>`; plain-text passwords are never stored.
- Server-side session authentication using an HTTP-only session cookie.
- Role-based API authorisation with ownership checks for event-specific operations.
- EF Core code-first model for the six Part 1 entities: `Users`, `Events`, `Categories`, `Enrolments`, `Results` and `Routes`.
- Swagger UI for viewing and testing API endpoints.
- Integration tests for unauthenticated requests, registration/login, organiser access and participant enrolment.
- GitHub Actions CI to restore, build, test, publish and upload the API artifact on every push.
- Event-day weather lookup through Open-Meteo. Weather may be unavailable for locations the provider cannot resolve or dates outside its forecast range.

The available distances are stored in `Categories.DistanceKm`, because one event can offer multiple race distances. This preserves the Part 1 ERD and SQL schema.

## Repository structure

```text
RaceDay/
├── .github/
│   └── workflows/
│       └── ci.yml
├── docs/
│   ├── RaceDay_ERD.png
│   ├── RaceDay_API_Endpoint_Plan.md
│   ├── RaceDay_Database.sql
│   └── README_docs.md
├── src/
│   └── RaceDay.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Models/
│       ├── Security/
│       ├── Services/
│       ├── Program.cs
│       ├── RaceDay.Api.csproj
│       └── appsettings.json
├── tests/
│   └── RaceDay.Api.Tests/
├── RaceDay.sln
└── README.md
```

## Requirements

Install:

- .NET 8 SDK
- Visual Studio 2022 with the ASP.NET and web development workload, or another .NET-compatible IDE
- SQL Server and SQL Server Management Studio (SSMS)
- Git

## Database setup

The default connection string is for SQL Server LocalDB on Windows:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

If your SQL Server instance uses a different name, update `ConnectionStrings:RaceDay` in `src/RaceDay.Api/appsettings.json` before running the API. Do not commit real passwords or production secrets to `appsettings.json`.

On first launch, the API uses the EF Core model to create the database schema when it does not already exist, then inserts sample data. The model is designed to align with the Part 1 ERD and `docs/RaceDay_Database.sql`. If you have already created `RaceDayDB` using the Part 1 script, ensure that its tables are present before running the API; `EnsureCreated` does not migrate an existing schema.

For production or ongoing schema changes, use EF Core migrations rather than relying on `EnsureCreated`. With the API project selected as the startup project, you can create a migration from the Package Manager Console or terminal. Example terminal commands from the repository root:

```bash
dotnet tool install --global dotnet-ef --version 8.0.31
dotnet ef migrations add InitialCreate --project src/RaceDay.Api/RaceDay.Api.csproj --startup-project src/RaceDay.Api/RaceDay.Api.csproj
```

After creating and reviewing the migration, change the startup database initialization to `db.Database.MigrateAsync()` and commit the generated migration files. Migrations should be introduced against a fresh database, or an existing `EnsureCreated` database must be backed up and deliberately baselined/recreated first; otherwise the initial migration will try to create tables that already exist. Do not run `EnsureCreated` and migrations together for the same schema lifecycle.

## Run the API

From the repository root:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet run --project src/RaceDay.Api/RaceDay.Api.csproj
```

Open the Swagger UI at the URL printed by the application, normally `https://localhost:7043/swagger` or `http://localhost:5043/swagger`.

### Development sample accounts

The first startup seeds these demonstration accounts. All use the sample password `RaceDay123!`:

| Role | Email |
|---|---|
| Organiser | `thabo.organiser@raceday.co.za` |
| Organiser | `lerato.organiser@raceday.co.za` |
| Participant | `sipho.participant@raceday.co.za` |
| Participant | `naledi.participant@raceday.co.za` |

These accounts are for local coursework testing only. Change or remove them before any real deployment. The session is stored in in-memory server-side cache for this starter project, so active sessions expire after 60 minutes and are cleared if the API process restarts.

## Using session authentication in Swagger

1. Execute `POST /api/auth/login` with a sample account's email and password.
2. The API sets an HTTP-only `.RaceDay.Session` cookie and stores the user ID, email and role in the server-side session.
3. Use the same Swagger browser session for protected endpoints. The browser sends the cookie automatically.
4. Execute `POST /api/auth/logout` to clear your session.

A Participant should receive `403 Forbidden` when attempting Organiser-only operations. An unauthenticated request to a protected endpoint should receive `401 Unauthorized`.

## Endpoint groups

| Area | Main routes |
|---|---|
| Authentication | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout` |
| User profile | `GET /api/users/me`, `PUT /api/users/me` |
| Events | `GET /api/events`, `GET /api/events/{id}`, `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` |
| Categories | `GET/POST /api/events/{eventId}/categories`, `PUT/DELETE /api/categories/{id}` |
| Enrolments | `POST /api/events/{eventId}/enrolments`, `GET /api/enrolments/me`, `DELETE /api/enrolments/{id}`, `GET /api/events/{eventId}/enrolments` |
| Results | `GET /api/results/me`, `GET /api/events/{eventId}/results`, `POST /api/events/{eventId}/results`, `PUT/DELETE /api/results/{id}` |
| Routes | `GET/POST /api/events/{eventId}/route`, `PUT /api/routes/{id}` |
| Weather | `GET /api/events/{eventId}/weather` |

The full endpoint plan is in [`docs/RaceDay_API_Endpoint_Plan.md`](docs/RaceDay_API_Endpoint_Plan.md).

## Run automated tests

From the repository root:

```bash
dotnet test RaceDay.sln --configuration Release
```

Tests use an EF Core in-memory database and do not require SQL Server. They check authentication requirements, registration/login/session behaviour, role restrictions for event creation and participant enrolment.

## Continuous Integration

GitHub Actions is configured in [`.github/workflows/ci.yml`](.github/workflows/ci.yml). It runs on every push and on pull requests targeting `main` or `develop`. The workflow restores dependencies, builds the solution, runs tests, publishes the API and uploads a `RaceDay-API` artifact.

### Successful CI build screenshot

After the first successful GitHub Actions run, open **Actions → RaceDay Continuous Integration**, capture a screenshot showing the green successful run, save it as `docs/ci-success.png`, and commit it. Then replace this paragraph with:

```markdown
![Successful RaceDay CI build](docs/ci-success.png)
```

Do not add a fabricated screenshot; use one from your own repository's successful run.

## Assessment and development status

This repository contains a starter implementation for Part 2. Before submitting, run the API and tests on your own machine, resolve any environment-specific errors, compare each endpoint against the approved Part 1 plan, and capture evidence required by the rubric. Add your presentation/video link if required by your assessment instructions.

The rubric requires at least 20 meaningful commits for Part 2. Commit your own work in logical increments as you implement, test, fix and document each feature; do not create empty or misleading commits just to reach the number.
