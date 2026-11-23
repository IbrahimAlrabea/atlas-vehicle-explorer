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

These are intended implementation mappings; automated tests for every case have not yet been completed.

## Frontend state

Angular uses RxJS `switchMap` for model-search request flows. Changing manufacturer clears dependent type and model state. A local text filter narrows the returned model list.

**Trade-off:** component-local state is simpler than introducing a global state library for this small application.

## Deployment decisions

The Docker Compose configuration runs two services: ASP.NET Core API and Angular served through Nginx. The API port is bound to localhost in the current configuration; Nginx provides browser-facing routing.

The demo is hosted on AWS EC2 (`t3.micro`, Ubuntu 24.04) using Docker Compose. The instance has approximately 1 GiB RAM and a 2 GiB swap file. Building Angular and .NET images on such a small instance would be resource-intensive, so GitHub Actions builds `linux/amd64` images and publishes them to public GitHub Container Registry packages. EC2 pulls the finished images and runs them with a separate `docker-compose.prod.yml` created on the host.

The frontend is publicly reachable on port 80. The API is bound to host loopback (`127.0.0.1:8080`), and Nginx forwards `/api/` and `/health` requests to the API container via Docker's internal network.

**AWS deployment status:** completed and manually verified at http://13.60.207.199/. This is a demonstration deployment over HTTP; it has no TLS certificate or stable domain, and the public IPv4 address may change after a stop/start. AWS pricing and Free Tier/Free Plan eligibility must be monitored for the account.

GitHub Actions automates **building and publishing** images, not the final EC2 rollout. The production Compose file is currently maintained on the host and documented in `README.md`, rather than committed to Git.

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
| GitHub Actions + GHCR | Reproducible off-instance image builds | Deployment to EC2 remains manual |
| Manual EC2 rollout | Simple for a small demo | No automatic rollout or rollback |

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
10. Confirmed frontend-to-backend communication and pushed the repository to GitHub.
11. Added a GitHub Actions workflow; the build-and-publish run completed successfully, and both API and web images appeared as public GHCR packages.
12. Provisioned an Ubuntu 24.04 AWS EC2 instance, configured a 2 GiB swap file, and installed Docker Engine and Docker Compose.
13. Pulled the prebuilt images from GHCR and confirmed both containers were `Up`.
14. Verified `curl -i http://127.0.0.1:8080/health` returned HTTP 200 on EC2.
15. Opened the public AWS site successfully and confirmed the application worked.

**Not yet verified/completed:** automated test suite, exhaustive upstream failure and invalid-input coverage, HTTPS, and automatic EC2 deployment. GitHub Actions image publishing and manual AWS deployment have been verified.

## AI assistance and human review

AI was used to suggest initial architecture, scaffold implementation, assist with Docker troubleshooting, and draft documentation. Suggestions were reviewed and adapted during development; actual Docker and browser testing was used to validate the working application.

### A real correction to AI-assisted setup guidance

Initial Docker setup guidance focused on installing Docker Desktop and enabling WSL 2. In practice, Docker Desktop reported:

```text
Virtualization is disabled in firmware
```

The issue was diagnosed by checking virtualization status in Windows and enabling Intel virtualization in BIOS/UEFI. The correction was verified by successfully running `docker run --rm hello-world` and starting both application services with Docker Compose.

**Scope:** this is a real correction to AI-assisted environment guidance, **not** a discovered bug in AI-generated application code. A code-level AI-generated defect is not claimed here. The documented mistake was an incomplete environment setup recommendation, corrected and verified during development.

## Next steps

1. Add backend tests using a mocked HTTP message handler and API integration tests.
2. Add Angular component tests and cover empty/error states.
3. Add request coalescing, bounded retries with jitter, and structured telemetry.
4. Improve manufacturer selection with virtualized autocomplete and keyboard accessibility.
5. Configure HTTPS, automated deployment, dependency scanning, and image pinning.
6. Add a stable domain, HTTPS/TLS, and AWS cost alerts.
7. Commit the production Compose configuration, add automated EC2 rollout/rollback, and pin images to immutable digests or release tags.

## Submission status

- Repository: https://github.com/IbrahimAlrabea/atlas-vehicle-explorer
- Local Docker application: manually verified
- AWS public URL: http://13.60.207.199/
- GitHub Actions: image build/publish verified; EC2 rollout remains manual
- Automated tests: not yet verified
