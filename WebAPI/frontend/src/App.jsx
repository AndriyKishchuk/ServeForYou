import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ManagerDashboardPage from "./pages/ManagerDashboardPage";
import ManagerCreateTaskPage from "./pages/ManagerCreateTaskPage";
import ManagerTaskDetailsPage from "./pages/ManagerTaskDetailsPage";
import ManagerProfilePage from "./pages/ManagerProfilePage";
import EmployeeTasksPage from "./pages/EmployeeTasksPage";
import EmployeeTaskDetailsPage from "./pages/EmployeeTaskDetailsPage";
import EmployeeProfilePage from "./pages/EmployeeProfilePage";
import CustomerOverviewPage from "./pages/CustomerOverviewPage";
import CustomerProfilePage from "./pages/CustomerProfilePage";
import ManagerDirectoryPage from "./pages/ManagerDirectoryPage";
import CustomerOrderRequestPage from "./pages/CustomerOrderRequestPage";
import CustomerTaskDetailsPage from "./pages/CustomerTaskDetailsPage";
import SettingsPage from "./pages/SettingsPage";

function PublicRoute({ children }) {
  const { isAuthenticated, isBootstrapping, redirectPath } = useAuth();

  if (isBootstrapping) {
    return <div className="screen-loader">Loading workspace...</div>;
  }

  if (isAuthenticated) {
    return <Navigate to={redirectPath} replace />;
  }

  return children;
}

function RootRedirect() {
  const { isAuthenticated, isBootstrapping, redirectPath } = useAuth();

  if (isBootstrapping) {
    return <div className="screen-loader">Preparing your workspace...</div>;
  }

  return <Navigate to={isAuthenticated ? redirectPath : "/login"} replace />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<RootRedirect />} />
      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginPage />
          </PublicRoute>
        }
      />
      <Route
        path="/register"
        element={
          <PublicRoute>
            <RegisterPage />
          </PublicRoute>
        }
      />
      <Route
        path="/manager/dashboard"
        element={
          <ProtectedRoute allowedRoles={["Manager"]}>
            <ManagerDashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/manager/tasks/new"
        element={
          <ProtectedRoute allowedRoles={["Manager"]}>
            <ManagerCreateTaskPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/manager/tasks/:taskId"
        element={
          <ProtectedRoute allowedRoles={["Manager"]}>
            <ManagerTaskDetailsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/manager/profile"
        element={
          <ProtectedRoute allowedRoles={["Manager"]}>
            <ManagerProfilePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/employee/tasks"
        element={
          <ProtectedRoute allowedRoles={["Employee"]}>
            <EmployeeTasksPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/employee/tasks/:taskId"
        element={
          <ProtectedRoute allowedRoles={["Employee"]}>
            <EmployeeTaskDetailsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/employee/profile"
        element={
          <ProtectedRoute allowedRoles={["Employee"]}>
            <EmployeeProfilePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customer/overview"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <CustomerOverviewPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customer/profile"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <CustomerProfilePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customer/managers"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <ManagerDirectoryPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customer/orders/new"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <CustomerOrderRequestPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/customer/orders/:taskId"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <CustomerTaskDetailsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/settings"
        element={
          <ProtectedRoute allowedRoles={["Admin", "Manager", "Employee"]}>
            <SettingsPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}
