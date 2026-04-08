# ServeForYou Project Documentation

## 1. Project Overview

ServeForYou is a task-routing service with three active roles:

- `Admin` acts as the customer
- `Manager` receives customer requests and routes them to an employee
- `Employee` performs the work and sends the result back to the manager

Main workflow:

1. `Admin -> Manager`
2. `Manager -> Employee`
3. `Employee -> Manager`
4. `Manager -> Admin`
5. `Admin -> Done`

The application consists of:

- `ASP.NET Web API` backend
- `React` frontend
- `PostgreSQL` database via Entity Framework Core
- local file storage under `wwwroot/uploads/...`

---

## 2. Solution Structure

### Root

- `DataBase/` - entities, EF Core configuration, migrations
- `WebAPI/WebAPI/` - ASP.NET backend
- `WebAPI/frontend/` - React frontend

### Important backend folders

- `Controllers/` - API endpoints
- `Factory/` - entity creation helpers
- `Models/Request/` - request DTOs
- `Models/Response/` - response DTOs
- `Services/` - JWT and file storage services

### Important frontend folders

- `src/pages/` - route pages
- `src/components/` - reusable UI blocks
- `src/context/` - auth context
- `src/api/` - axios API layer
- `src/utils/` - UI helpers such as status labels

---

## 3. Core Domain Model

### Users

- `Id`
- `Name`
- `Surname`
- `Email`
- `PasswordHash`
- `Role`
- `CompanyId`

Roles:

- `Admin`
- `Manager`
- `Employee`

### Companies

- `Id`
- `CompanyName`

### Addresses

- `Id`
- `CompanyId`
- `City`
- `Street`

### Tasks

- `Id`
- `TaskName`
- `Description`
- `CreatedByUserId`
- `ManagerUserId`
- `AssignedToUserId`
- `Status`
- `CreatedAt`

### TaskFiles

- `Id`
- `TaskId`
- `UploadedByUserId`
- `FileName`
- `StoredFileName`
- `FilePath`
- `ContentType`
- `FileSize`
- `UploadedAt`
- `Category`

---

## 4. Task Status Flow

Current statuses:

- `New`
- `InProgress`
- `SubmittedToManager`
- `ReturnedToAdmin`
- `Done`

Meaning:

- `New` - task created and waiting for action
- `InProgress` - employee started work
- `SubmittedToManager` - employee finished and sent result to manager
- `ReturnedToAdmin` - manager reviewed and returned result to customer
- `Done` - customer confirmed everything is okay

Allowed flow:

1. `Admin` creates customer request -> task is assigned to selected `Manager`
2. `Manager` assigns task to `Employee`
3. `Employee` changes `New -> InProgress`
4. `Employee` changes `InProgress -> SubmittedToManager`
5. `Manager` changes `SubmittedToManager -> ReturnedToAdmin`
6. `Admin` changes `ReturnedToAdmin -> Done`

---

## 5. Role Permissions

### Admin

- register and login
- browse managers
- create customer requests
- see only own tasks
- see result files only after manager returns task
- confirm returned task as `Done`
- update own profile
- change own password
- request account deletion

### Manager

- login
- view manager inbox
- upload instruction files
- assign tasks to employees
- review employee result files
- return completed work to customer
- update own profile
- change own password
- request account deletion

### Employee

- login
- view only own assigned tasks
- download attachment files
- upload result files
- move status `New -> InProgress -> SubmittedToManager`
- update own profile
- change own password
- request account deletion

---

## 6. Main Backend Endpoints

### Auth

- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/me`

### User

- `GET /api/user/me`
- `PUT /api/user/me`
- `PATCH /api/user/me/password`
- `DELETE /api/user/me`
- `GET /api/user/managers`
- `GET /api/user/company`

### Task

- `GET /api/task/company/all`
- `GET /api/task/manager/inbox`
- `GET /api/task/assigned/{employeeId}`
- `GET /api/task/{id}`
- `POST /api/task/customer-request`
- `POST /api/task`
- `PATCH /api/task/{id}/assign`
- `PATCH /api/task/{id}/status`
- `PATCH /api/task/{id}/return-to-customer`
- `PATCH /api/task/{id}/complete`
- `DELETE /api/task/{id}`

### Files

- `POST /api/tasks/{taskId}/files`
- `GET /api/tasks/{taskId}/files`
- `GET /api/files/{id}/download`
- `DELETE /api/files/{id}`

---

## 7. File Storage

Files are stored locally in:

- `wwwroot/uploads/{companyId}/{taskId}/`

Implementation is handled by:

- `WebAPI/WebAPI/Services/FileStorageService.cs`

Rules:

- only selected MIME types are allowed
- max file size is `20 MB`
- manager uploads `Attachment`
- employee uploads `Result`
- admin cannot access result files until task status becomes `ReturnedToAdmin`

---

## 8. Frontend Pages

### Public

- `/login`
- `/register`

### Admin / Customer

- `/customer/overview`
- `/customer/managers`
- `/customer/orders/new`
- `/customer/orders/:taskId`
- `/customer/profile`

### Manager

- `/manager/dashboard`
- `/manager/tasks/new`
- `/manager/tasks/:taskId`
- `/manager/profile`

### Employee

- `/employee/tasks`
- `/employee/tasks/:taskId`
- `/employee/profile`

### Shared

- `/settings`

---

## 9. Settings Page

Settings page includes:

- account overview
- edit profile form
- change password form
- delete account action

Delete account is protected:

- user cannot delete own account if it is still linked to tasks
- user cannot delete own account if uploaded files are linked to that account

---

## 10. Authentication

Authentication uses JWT.

Key points:

- token is generated on login and register
- frontend stores auth data in local storage
- `AuthContext` restores session on refresh using `/auth/me` and `/user/me`
- redirects depend on role:
  - `Admin -> /customer/overview`
  - `Manager -> /manager/dashboard`
  - `Employee -> /employee/tasks`

---

## 11. Default Company Rule

The system currently works with a single default company:

- `ServeForYou`

Registration does not require user-side company selection.
Backend ensures that the default company exists.

---

## 12. Running the Project

### Backend

Run from root:

```powershell
dotnet build WebAPI\WebAPI\WebAPI.csproj
dotnet run --project WebAPI\WebAPI\WebAPI.csproj
```

### Frontend

Run from:

- `WebAPI/frontend`

Commands:

```powershell
npm install
npm start
```

Production build:

```powershell
npm run build
```

---

## 13. Important Notes

- There are older unused frontend pages still present in `src/pages/` such as `ManagerBoard.jsx`, `EmployeeBoard.jsx`, `ManagerWorkspace.jsx`, `EmployeeWorkspace.jsx`, and `AdminBoard.jsx`
- current production flow uses the newer route pages instead
- status display in customer UI is mapped to readable labels such as `Ready for review` and `Completed`

---

## 14. Suggested Next Improvements

- remove unused legacy frontend pages
- unify status label formatting across manager and employee pages
- move all profile-related functionality fully into `/settings`
- add validation messages on all forms
- add integration tests for task flow
- add database seed script for demo users
- add comments/history to tasks
- add notifications for status changes

---

## 15. Key Files

Backend:

- `WebAPI/WebAPI/Controllers/TaskController.cs`
- `WebAPI/WebAPI/Controllers/FileController.cs`
- `WebAPI/WebAPI/Controllers/UserController.cs`
- `WebAPI/WebAPI/Controllers/AuthController.cs`
- `WebAPI/WebAPI/Services/FileStorageService.cs`
- `WebAPI/WebAPI/Program.cs`

Database:

- `DataBase/DataBase/Users/Tasks.cs`
- `DataBase/DataBase/Users/User.cs`
- `DataBase/DataBase/Users/TaskFile.cs`
- `DataBase/DataBase/Configure/TaskConfigure.cs`
- `DataBase/DataBase/Configure/TaskFileConfigure.cs`

Frontend:

- `WebAPI/frontend/src/App.jsx`
- `WebAPI/frontend/src/context/AuthContext.jsx`
- `WebAPI/frontend/src/api/api.js`
- `WebAPI/frontend/src/components/WorkspaceLayout.jsx`
- `WebAPI/frontend/src/pages/SettingsPage.jsx`

