# ServeForYou

ServeForYou is a task-routing platform with three main roles:

- `Admin` creates service requests as the customer
- `Manager` receives requests and routes them to employees
- `Employee` performs the work and sends results back through the manager

Main workflow:

`Admin -> Manager -> Employee -> Manager -> Admin -> Done`

## Stack

- `ASP.NET Web API`
- `React`
- `Entity Framework Core`
- `PostgreSQL`
- `Docker Compose`

## Project Structure

```text
DataBase/
  DataBase/

WebAPI/
  WebAPI/
  frontend/
```

- `DataBase/DataBase` - entities, EF Core configuration, migrations
- `WebAPI/WebAPI` - backend API
- `WebAPI/frontend` - React frontend

## Main Features

- JWT authentication
- role-based access and redirects
- customer request flow
- manager inbox and employee routing
- file attachments and result uploads
- settings page with profile edit, password change, and self-delete
- Docker support for db, backend, and frontend

## Run Locally with Docker

1. Copy `.env.example` to `.env`
2. Update the values inside `.env`
3. Run:

```powershell
docker compose up --build
```

4. Open:

- Frontend: [http://localhost:3000](http://localhost:3000)

## Run Without Docker

### Backend

```powershell
dotnet build WebAPI\WebAPI\WebAPI.csproj
dotnet run --project WebAPI\WebAPI\WebAPI.csproj
```

### Frontend

```powershell
cd WebAPI\frontend
npm install
npm start
```

## Documentation

- [PROJECT_DOCUMENTATION.md](C:\Users\andri\OneDrive\Documents\New project\PROJECT_DOCUMENTATION.md)

