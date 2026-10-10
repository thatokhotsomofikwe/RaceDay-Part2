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




## Database setup

The default connection string is for SQL Server LocalDB on Windows:


Server=(localdb)\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True


## Run the API

From the repository root:



Open the Swagger UI at the URL printed by the application, normally `https://localhost:7043/swagger` or `http://localhost:5043/swagger`.

### Development sample accounts

Example password RaceDay123!

|example accounts:
| Organiser | `thabo.organiser@raceday.co.za` |
| Organiser | `lerato.organiser@raceday.co.za` |
| Participant | `sipho.participant@raceday.co.za` |
| Participant | `naledi.participant@raceday.co.za` |



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




GitHub Actions is configured in [`.github/workflows/ci.yml`](.github/workflows/ci.yml). It runs on every push and on pull requests targeting `main` or `develop`. The workflow restores dependencies, builds the solution, runs tests, publishes the API and uploads a `RaceDay-API` artifact.

### Successful CI build screenshot



###YouTube Unlisted Link


