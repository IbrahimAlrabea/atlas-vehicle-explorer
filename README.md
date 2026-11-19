# ATLAS — Vehicle Explorer

A distinct, editorial-style vehicle discovery application. Angular 18 + ASP.NET Core 8, backed by the public NHTSA vPIC API.

## Prerequisites
- Docker Engine with Compose plugin (recommended)
- Alternatively: .NET 8 SDK, Node.js 20 and npm

## Quick start (Docker)
1. Clone the repository.
2. Run `docker compose up --build -d` from the repository root.
3. Visit `http://localhost` (set `WEB_PORT=8081` if port 80 is occupied).
4. API health: `http://localhost:8080/health` (bound to localhost only).
5. Stop with `docker compose down`.

## Local development
In terminal 1:
```sh
cd backend/VehicleExplorer.Api
dotnet run --urls http://localhost:8080
```
In terminal 2:
```sh
cd frontend
npm install
npm start
```
Open `http://localhost:4200`. Angular's proxy forwards `/api` to `localhost:8080`.

## API
- `GET /api/vehicles/makes`
- `GET /api/vehicles/makes/{makeId}/types`
- `GET /api/vehicles/models?makeId=474&year=2015`
- `GET /api/vehicles/models?makeId=474&year=2015&type=car`

## Smoke test
1. Select Honda and year 2015.
2. Choose Passenger Car and click Explore Models.
3. Expect Accord and Civic among results.
4. Change year, then make, and verify old results clear.
5. Check a make/year combination with no models, and invalid query parameters.

## AWS EC2 deployment (manual)
1. Check AWS Free Plan eligibility and pricing in your own account first. Set a budget alert; do not assume EC2 is free.
2. Create an EC2 Linux instance sized for Docker builds; configure its security group for SSH from **your IP only**, and HTTP (80) from the internet. Use an SSH key, not passwords.
3. Install Docker Engine and the Docker Compose plugin using the official instructions for your chosen AMI.
4. Clone your public repository on the instance. Run `docker compose up --build -d`.
5. Visit `http://<public-ip>`. For a real public deployment, add a domain and HTTPS reverse proxy (e.g. Caddy) before sharing broadly.
6. Verify `/health` locally on the instance and check `docker compose logs` if needed.
7. Avoid committing AWS credentials, `.env`, SSH private keys or access tokens.

## Known limits
- The upstream catalog is large; the manufacturer dropdown filters locally and shows the first 120 matching entries. Type aliases cover common categories; unsupported types are disabled.
- No database or authentication because the assignment is read-only and public.
- Caching is per process and does not prevent simultaneous first-time upstream requests.
- Production should have HTTPS, tighter ingress controls, structured telemetry, and automated integration tests.

## Submission
Provide your public GitHub repository URL and the live AWS URL. Verify both from an incognito window. Commit changes incrementally rather than uploading one final commit.

## Open in Visual Studio and VS Code

**Backend (Microsoft Visual Studio 2022):** Open `backend/VehicleExplorer.sln`. Install the .NET 8 SDK and ASP.NET/web development workload. Set `VehicleExplorer.Api` as startup project. Run using HTTP (default launch settings may need to be configured), or from a terminal with `dotnet run --urls http://localhost:8080` in `backend/VehicleExplorer.Api`.

**Frontend (Visual Studio Code):** Open the `frontend` folder, run `npm install`, then `npm start` in the integrated terminal. Browse `http://localhost:4200`. Start the backend on port 8080 first, since the Angular development proxy expects it there.

## Refactoring notes

Backend responsibilities are separated into `Endpoints`, `Services`, `Clients`, `DTOs`, and `Middleware`.
The public API routes and JSON property names are unchanged; the Angular frontend needs no changes.
The in-memory cache is per-process and intentionally simple for this assignment.

**Verification:** run `dotnet build backend/VehicleExplorer.sln`, then run the API and test `/api/vehicles/makes`, `/api/vehicles/makes/474/types`, and `/api/vehicles/models?makeId=474&year=2015&type=car`.
