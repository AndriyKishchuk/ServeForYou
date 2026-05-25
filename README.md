# ServeForYou

ServeForYou is a service workflow platform for handling customer requests inside one company. The system supports three roles:

- `Admin` creates and reviews customer requests
- `Manager` receives requests, assigns them to employees, and returns results
- `Employee` completes assigned work and uploads result files

Core workflow:

`Admin -> Manager -> Employee -> Manager -> Admin -> Done`

## What is in this repository

- `DataBase/DataBase` - domain entities, EF Core configuration, migrations
- `WebAPI/WebAPI` - ASP.NET Core Web API
- `WebAPI/frontend` - React frontend
- `docker-compose.yml` - local full-stack environment with PostgreSQL, Redis, backend, and frontend

## Tech stack

- `ASP.NET Core Web API` on `.NET 10`
- `Entity Framework Core`
- `PostgreSQL`
- `Redis`
- `React 18`
- `Docker Compose`
- `xUnit`, `Moq`, `FluentAssertions`, `React Testing Library`

## Main features

- JWT authentication and role-based access
- customer request creation flow
- manager inbox and employee assignment flow
- employee task execution with status updates
- file attachments and result uploads
- profile editing, password change, and self-delete
- local file storage in `wwwroot/uploads`
- automatic EF Core migrations on backend startup

## Status flow

- `New`
- `InProgress`
- `SubmittedToManager`
- `ReturnedToAdmin`
- `Done`

## Project structure

```text
.
|- DataBase/
|  \- DataBase/
|- WebAPI/
|  |- WebAPI/
|  |- WebAPI.Tests/
|  \- frontend/
|- docker-compose.yml
\- PROJECT_DOCUMENTATION.md
```

## Key API areas

- `Auth`: login, register, current session
- `User`: profile management, password change, manager/company lists
- `Task`: customer requests, manager inbox, assignment, completion flow
- `File`: upload, list, download, delete task files
- `Company` and `Adress`: admin CRUD endpoints

The backend also exposes OpenAPI and Scalar API reference in development mode.

## Frontend routes

Public:

- `/login`
- `/register`

Admin:

- `/customer/overview`
- `/customer/managers`
- `/customer/orders/new`
- `/customer/orders/:taskId`

Manager:

- `/manager/dashboard`
- `/manager/tasks/new`
- `/manager/tasks/:taskId`

Employee:

- `/employee/tasks`
- `/employee/tasks/:taskId`

Shared:

- `/settings`

## Environment variables

Copy `.env.example` to `.env` and adjust values if needed:

```env
POSTGRES_DB=serveforyou_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me

JWT_SECRET=change_me_super_secret_key
JWT_ISSUER=ServeForYou
JWT_AUDIENCE=ServeForYouUsers

BACKEND_PORT=8080
FRONTEND_PORT=3000
```

Notes:

- Docker Compose uses `.env` for PostgreSQL, JWT, and published ports
- backend Redis connection defaults to `localhost:6379` outside Docker
- current `WebAPI/WebAPI/appsettings.json` contains local development values, so environment variables should be preferred for real runs

## Run with Docker

Prerequisite: Docker Desktop or Docker Engine with Compose support.

1. Create `.env` from `.env.example`
2. Start the stack:

```powershell
docker compose up --build
```

Available services:

- Frontend: [http://localhost:3000](http://localhost:3000)
- Backend: [http://localhost:8080](http://localhost:8080)
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

The backend waits for PostgreSQL, applies migrations automatically, and creates the default company `ServeForYou` if it does not exist.

## Run locally without Docker

Prerequisites:

- `.NET 10 SDK`
- `Node.js 18+`
- local `PostgreSQL`
- local `Redis`

### 1. Start infrastructure

You can run PostgreSQL and Redis locally, or start only those services with Docker:

```powershell
docker compose up db redis
```

### 2. Run backend

```powershell
dotnet build WebAPI\WebAPI\WebAPI.csproj
dotnet run --project WebAPI\WebAPI\WebAPI.csproj
```

The API serves static uploaded files from `WebAPI/WebAPI/wwwroot/uploads`.

### 3. Run frontend

```powershell
cd WebAPI\frontend
npm install
npm start
```

The frontend uses `/api` as its base URL, so in local development it should be served together with a matching backend setup or proxy/container routing.

## Tests

Backend tests:

```powershell
dotnet test WebAPI\WebAPI.Tests\WebAPI.Tests.csproj
```

Frontend tests:

```powershell
cd WebAPI\frontend
npm test
```

## Useful files

- [PROJECT_DOCUMENTATION.md](C:\Users\andri\OneDrive\Documents\New project\PROJECT_DOCUMENTATION.md)
- [docker-compose.yml](C:\Users\andri\OneDrive\Documents\New project\docker-compose.yml)
- [WebAPI/WebAPI/Program.cs](C:\Users\andri\OneDrive\Documents\New project\WebAPI\WebAPI\Program.cs)
- [WebAPI/frontend/src/App.jsx](C:\Users\andri\OneDrive\Documents\New project\WebAPI\frontend\src\App.jsx)

## Current notes

- the project currently assumes one default company named `ServeForYou`
- uploaded files are stored locally, not in cloud storage
- there are older frontend pages still present in `WebAPI/frontend/src/pages`, but the current app routes use the newer workspace pages
