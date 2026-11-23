# ATLAS — Vehicle Explorer

A vehicle discovery application built with Angular 18 and ASP.NET Core 8 Web API, powered by the public NHTSA vPIC API.

Users can select a manufacturer, model year, and optional vehicle type to explore matching models.

## Technology stack

- Frontend: Angular 18, TypeScript, RxJS; Nginx for container hosting
- Backend: ASP.NET Core 8 Web API, controllers, typed HttpClient, IMemoryCache
- Infrastructure: Docker Compose, GitHub Actions, GitHub Container Registry (GHCR), AWS EC2 (Ubuntu 24.04)
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

## Live AWS deployment

**Live demo:** http://13.60.207.199/

The application is deployed on an AWS EC2 Ubuntu 24.04 instance using Docker Compose. The deployment uses prebuilt Linux/amd64 images from GitHub Container Registry (GHCR), avoiding memory-intensive Angular and .NET builds on the small EC2 instance.

- API image: `ghcr.io/ibrahimalrabea/atlas-vehicle-explorer-api:latest`
- Web image: `ghcr.io/ibrahimalrabea/atlas-vehicle-explorer-web:latest`
- Public entry point: Nginx on HTTP port 80
- API host binding: `127.0.0.1:8080` (not directly exposed publicly)
- Nginx proxies `/api/` and `/health` to the API container over the Compose network.

### Deploy on an Ubuntu EC2 instance

Install Docker Engine and the Docker Compose plugin using the [official Docker Ubuntu instructions](https://docs.docker.com/engine/install/ubuntu/). Allow inbound HTTP on port 80 and restrict administrative access. Clone the repository:

```bash
git clone https://github.com/IbrahimAlrabea/atlas-vehicle-explorer.git
cd atlas-vehicle-explorer
```

Create `docker-compose.prod.yml` in the project root:

```yaml
services:
  api:
    image: ghcr.io/ibrahimalrabea/atlas-vehicle-explorer-api:latest
    restart: unless-stopped
    environment:
      ASPNETCORE_URLS: http://+:8080
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "127.0.0.1:8080:8080"

  web:
    image: ghcr.io/ibrahimalrabea/atlas-vehicle-explorer-web:latest
    restart: unless-stopped
    depends_on:
      - api
    ports:
      - "80:80"
```

Then pull and start the prebuilt images:

```bash
sudo docker compose -f docker-compose.prod.yml config
sudo docker compose -f docker-compose.prod.yml pull
sudo docker compose -f docker-compose.prod.yml up -d
sudo docker compose -f docker-compose.prod.yml ps
curl -i http://127.0.0.1:8080/health
curl -i http://127.0.0.1/api/vehicles/makes
```

`docker-compose.prod.yml` was created on the EC2 host for the verified deployment; it is documented here for reproducibility and is not claimed to be committed to the repository.

### Container image publishing

The repository includes `.github/workflows/main.yml`. On pushes to `main` (or manual dispatch), GitHub Actions builds separate `api` and `web` images for `linux/amd64` and publishes `latest` and commit-derived tags to GHCR. Both published packages are public. **This automates image builds and publishing, not deployment to EC2**: updating the live server still requires pulling the new images and recreating the containers.

The live URL currently uses an automatically assigned EC2 public IPv4 address, which can change after an instance stop/start. HTTPS and a stable domain are not configured.

## Security and limitations

- Never commit credentials, SSH keys, access tokens, or `.env` secrets.
- API access is limited to localhost by the current Docker Compose port binding; Nginx handles browser-facing API requests.
- Manufacturer search limits visible results to the first 120 matches.
- Vehicle type alias coverage is incomplete.
- IMemoryCache is per-process; simultaneous misses may duplicate upstream calls.
- Automated tests have not been verified. GitHub Actions image build/publish CI is working; EC2 deployment is manual.
- This demo is served over HTTP, not HTTPS; do not submit sensitive information. Production should include TLS, observability, dependency scanning, and tighter ingress controls.

## Submission

- GitHub: https://github.com/IbrahimAlrabea/atlas-vehicle-explorer
- Live AWS URL: http://13.60.207.199/

Verify both public links in an incognito browser before submission. The live demo was opened successfully and the API health endpoint returned HTTP 200 on EC2. Make real incremental commits as improvements are completed.
