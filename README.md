# ATLAS — Vehicle Explorer

A vehicle discovery application built with Angular 18 and ASP.NET Core 8 Web API, powered by the public NHTSA vPIC API.

Users can select a manufacturer, model year, and optional vehicle type to explore matching models.

## Technology stack

- Frontend: Angular 18, TypeScript, RxJS; Nginx for container hosting
- Backend: ASP.NET Core 8 Web API, controllers, typed HttpClient, IMemoryCache
- Infrastructure: Docker Compose, GitHub; AWS EC2 deployment planned
- Upstream data: https://vpic.nhtsa.dot.gov/api/

## Prerequisites

- Recommended: Docker Engine/Desktop with Docker Compose
- Local development: .NET 8 SDK, a Node.js version supported by the Angular toolchain, and npm

## Quick start with Docker

```bash
git clone https://github.com/IbrahimAlrabea/atlas-vehicle-explorer.git
cd atlas-vehicle-explorer
docker compose up --build -d
docker compose ps
```

- Frontend: http://localhost
- API health: http://localhost:8080/health
- Makes endpoint: http://localhost:8080/api/vehicles/makes

The API port is bound to localhost by default. If port 80 is in use, on Git Bash run:

```bash
WEB_PORT=8081 docker compose up --build -d
```

Then visit http://localhost:8081.

```bash
docker compose logs -f
docker compose down
```

## Local development

**Backend (Visual Studio 2022):** Open `backend/VehicleExplorer.sln`, select `VehicleExplorer.Api` as the startup project, or run:

```bash
cd backend/VehicleExplorer.Api
dotnet restore
dotnet run --urls http://localhost:8080
```

**Frontend (VS Code):** In another terminal:

```bash
cd frontend
npm install
npm start
```

Visit http://localhost:4200. The Angular development proxy forwards `/api` to localhost:8080. Start the backend first.

## API endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/vehicles/makes` | List manufacturers |
| GET | `/api/vehicles/makes/{makeId}/types` | List types for a manufacturer |
| GET | `/api/vehicles/models?makeId=474&year=2015` | Models for make and year |
| GET | `/api/vehicles/models?makeId=474&year=2015&type=car` | Models filtered by type |
| GET | `/health` | Health check |

## Architecture

The backend uses **controller-based endpoints**, not Minimal APIs. Responsibilities are separated into:

- `Controllers/`: HTTP routing and request handling
- `Services/`: vehicle catalog coordination and caching
- `Clients/`: typed NHTSA HTTP integration and upstream contracts
- `DTOs/`: application response contracts
- `Middleware/`: centralized exception handling
- `Program.cs`: dependency injection and HTTP pipeline setup

The Angular frontend lives in `frontend/src/app/`. Nginx serves the built frontend and proxies API traffic in the containerized configuration.

## Engineering choices

- Typed HttpClient isolates the upstream NHTSA dependency.
- IMemoryCache reduces repeat requests (makes: 12h; types: 6h; models: 3h).
- No database or authentication is needed for the read-only public catalog.
- Explicit aliases support common vehicle type filters; unsupported mappings are not silently submitted.
- Centralized exception handling returns controlled HTTP error responses.

See `DECISIONS.md` for rationale and trade-offs.

## Manual verification performed

- Built and started services with `docker compose up --build`.
- Restarted in detached mode with `docker compose up -d` and checked `docker compose ps`.
- Verified `/health` returned HTTP 200.
- Opened the Angular frontend in the browser.
- Selected Honda and year 2015 and verified model discovery worked through the backend.
- Inspected startup and request logs with `docker compose logs -f`.
- Verified Docker itself using the official `hello-world` image.

These are manual checks, not automated test coverage. Additional recommended tests include invalid inputs, empty results, upstream errors/timeouts, rapid selection changes, and accessibility.

## AWS EC2 deployment (planned)

1. Review your account's current AWS pricing and Free Tier/Free Plan eligibility; configure a billing alert.
2. Provision a Linux EC2 instance with sufficient resources for the chosen build strategy.
3. Restrict SSH to your IP; allow HTTP only as needed for the demonstration.
4. Install Docker Engine and the Docker Compose plugin from official instructions for the selected OS.
5. Clone the public repository and run `docker compose up --build -d`.
6. Check the application at `http://<public-ip>` and verify `/health` locally on the instance.
7. Add HTTPS and a suitable reverse proxy before treating the public deployment as production-ready.

**Status:** AWS deployment has not yet been completed. No public AWS URL is claimed.

## Security and limitations

- Never commit credentials, SSH keys, access tokens, or `.env` secrets.
- API access is limited to localhost by the current Docker Compose port binding; Nginx handles browser-facing API requests.
- Manufacturer search limits visible results to the first 120 matches.
- Vehicle type alias coverage is incomplete.
- IMemoryCache is per-process; simultaneous misses may duplicate upstream calls.
- No automated tests or CI/CD have been verified.
- Production should include HTTPS, observability, dependency scanning, and tighter ingress controls.

## Submission

- GitHub: https://github.com/IbrahimAlrabea/atlas-vehicle-explorer
- Live AWS URL: pending deployment

Verify the public links in an incognito browser before submission. Make real incremental commits as improvements are completed.
