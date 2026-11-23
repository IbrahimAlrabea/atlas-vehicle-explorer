# ATLAS — Engineering Decisions and Trade-offs

## Product concept

ATLAS is a read-only vehicle discovery experience with a dark editorial visual style, restrained lime accents, and a model directory rather than a generic CRUD dashboard. The design aims to be distinctive; originality cannot be guaranteed.

## Technology selection

- **Angular 18:** component-based UI, TypeScript, and RxJS for asynchronous state.
- **ASP.NET Core 8 Web API:** controller routing, built-in DI, typed HttpClient, and centralized exception handling.
- **Docker Compose:** reproducible local startup of the API and Nginx-served frontend.
- **NHTSA vPIC:** public upstream source of truth for manufacturers, vehicle types, and models.

## Architecture

```text
Browser → Angular/Nginx → ASP.NET Core VehiclesController
                                  ↓
                           IVehicleCatalog
                                  ↓
                         VehicleCatalogService
                                  ↓
                             INhtsaClient
                                  ↓
                            NHTSA vPIC API
```

The backend uses **controller-based REST endpoints**, not Minimal APIs. Controllers handle HTTP concerns, services coordinate catalog retrieval and caching, and the typed client isolates upstream transport. DTOs avoid exposing upstream contracts directly.

No database, ORM, repository layer, CQRS, or authentication is introduced because the application is read-only and has no persistent user data. These abstractions would add complexity without clear benefit for this scope.

## Vehicle type semantics

NHTSA returns descriptive vehicle-type labels, while model queries use textual type filters. Explicit aliases support common categories. Unknown types can be displayed but are not silently mapped to incorrect filters. Type selection is optional.

**Trade-off:** alias coverage is limited and should be validated against the full upstream taxonomy.

## Caching

| Resource | IMemoryCache duration |
|---|---|
| Makes | 12 hours |
| Types | 6 hours |
| Models | 3 hours |

A local cache is simple for a single backend instance. Multiple replicas would have independent caches, restarts clear cached entries, and concurrent misses can trigger duplicate upstream requests. Redis and request coalescing are possible future improvements.

## Error handling

Centralized exception handling is designed to map invalid inputs to HTTP 400, upstream HTTP failures to 502, upstream timeouts to 504, malformed upstream JSON to 502, and unexpected failures to 500. Sensitive internal exception details should not be returned to clients.

The `ArgumentException` → HTTP 400 path was verified manually with an unsupported vehicle type. Automated tests for every error case have not yet been completed.

## Frontend state

Angular uses RxJS `switchMap` for model-search request flows. Changing manufacturer clears dependent type and model state. A local text filter narrows the returned model list.

**Trade-off:** component-local state is simpler than introducing a global state library for this small application.

## Deployment decisions

The Docker Compose configuration runs two services: ASP.NET Core API and Angular served through Nginx. The API port is bound to localhost in the current configuration; Nginx provides browser-facing routing.

AWS EC2 is the planned demo target, but pricing and Free Tier eligibility must be checked for the actual account. Building images on a small instance can be resource-intensive. Public HTTP is only a demo baseline; HTTPS is needed for production-quality deployment.

**AWS deployment status:** not yet completed.

## Trade-offs and known gaps

| Choice | Benefit | Limitation |
|---|---|---|
| Controllers | Clear HTTP boundary | More structure than Minimal APIs |
| Typed HttpClient | Isolates NHTSA integration | Dependent on upstream availability |
| IMemoryCache | Low operational complexity | Per-instance and nonpersistent |
| No database | Small, focused solution | No persistent application state |
| Type aliases | Predictable filters | Incomplete taxonomy coverage |
| Component-local state | Easy to follow | May not scale to larger feature sets |
| Docker Compose | Reproducible setup | Not an orchestration platform |
| Manual EC2 deployment | Straightforward demo | No CI/CD or automatic rollout |

## Verification performed

The following checks were actually completed:

1. Installed Docker Desktop and configured WSL 2.
2. Enabled hardware virtualization in BIOS/UEFI after Docker Desktop reported it was disabled.
3. Ran the official Docker `hello-world` container successfully.
4. Built and started the application using `docker compose up --build`.
5. Started both services in detached mode using `docker compose up -d`.
6. Confirmed both containers were `Up` using `docker compose ps`.
7. Inspected service logs using `docker compose logs -f`.
8. Confirmed the API `/health` endpoint returned HTTP 200.
9. Opened the Angular frontend and successfully explored Honda models for year 2015.
10. Confirmed frontend-to-backend communication and pushed the initial repository commit to GitHub.
11. Ran `dotnet build backend/VehicleExplorer.sln` successfully with zero warnings and zero errors after resolving a local executable file lock.
12. Rebuilt the Docker images and confirmed both services were running with `docker compose ps`.
13. Verified `GET /health` returned HTTP 200; invalid `makeId` and `year` returned HTTP 400 from controller validation.
14. Verified `GET /api/vehicles/models?makeId=474&year=2020&type=INVALID_TYPE` returned HTTP 400 with `application/problem+json` and title `Invalid request parameters`.
15. Committed and pushed the exception-handling fix as commit `0beae99`.

**Not yet verified:** automated tests, full upstream failure coverage, AWS deployment, HTTPS, and CI/CD.

## AI assistance and human review

AI was used to suggest initial architecture, scaffold implementation, assist with Docker troubleshooting, and draft documentation. Suggestions were reviewed and adapted during development; actual Docker and browser testing was used to validate the working application.

### A real correction to AI-assisted setup guidance

Initial Docker setup guidance focused on installing Docker Desktop and enabling WSL 2. In practice, Docker Desktop reported:

```text
Virtualization is disabled in firmware
```

The issue was diagnosed by checking virtualization status in Windows and enabling Intel virtualization in BIOS/UEFI. The correction was verified by successfully running `docker run --rm hello-world` and starting both application services with Docker Compose.

**Scope:** this is a real correction to AI-assisted environment guidance, not a bug in AI-generated application code.

### AI-assisted exception handling correction

During AI-assisted review of the API exception handler, a regression was identified in the proposed mapping: `ArgumentException` was omitted, so unsupported vehicle types would fall through to the generic HTTP 500 branch. The corrected handler restores HTTP 400 mapping while retaining structured logging and Problem Details responses.

**Verification performed:**

- `dotnet build backend/VehicleExplorer.sln` succeeded with zero warnings and errors.
- `docker compose up --build -d` rebuilt and started the containers successfully.
- `GET /health` returned HTTP 200.
- Invalid `makeId` and `year` returned HTTP 400 through controller validation.
- `GET /api/vehicles/models?makeId=474&year=2020&type=INVALID_TYPE` returned HTTP 400 with `Content-Type: application/problem+json`, title `Invalid request parameters`, and instance `/api/vehicles/models`.

The regression was identified by code review. The corrected behavior was verified by a real API request; the prior HTTP 500 behavior was inferred from the earlier exception mapping rather than reproduced in a separate runtime test.

**Code fix commit:** `0beae99` — `fix: restore bad request mapping and improve exception logging`.

## Next steps

1. Add backend tests using a mocked HTTP message handler and API integration tests.
2. Add Angular component tests and cover empty/error states.
3. Add request coalescing, bounded retries with jitter, and structured telemetry.
4. Improve manufacturer selection with virtualized autocomplete and keyboard accessibility.
5. Configure HTTPS, automated deployment, dependency scanning, and image pinning.
6. Deploy to AWS, validate the public endpoint, and update the submission URL.

## Submission status

- Repository: https://github.com/IbrahimAlrabea/atlas-vehicle-explorer
- Local Docker application: manually verified
- AWS public URL: pending
- Automated tests: not yet verified
