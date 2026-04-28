import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";
import { useAuth } from "../context/AuthContext";

jest.mock("../context/AuthContext", () => ({
  useAuth: jest.fn(),
}));

function renderProtectedRoute(authValue, allowedRoles = ["Manager"]) {
  useAuth.mockReturnValue(authValue);

  return render(
    <MemoryRouter initialEntries={["/manager/dashboard"]}>
      <Routes>
        <Route
          path="/manager/dashboard"
          element={
            <ProtectedRoute allowedRoles={allowedRoles}>
              <div>Protected content</div>
            </ProtectedRoute>
          }
        />
        <Route path="/login" element={<div>Login screen</div>} />
        <Route path="/employee/tasks" element={<div>Employee home</div>} />
      </Routes>
    </MemoryRouter>
  );
}

describe("ProtectedRoute", () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  it("shows a loader while auth state is bootstrapping", () => {
    renderProtectedRoute({
      auth: null,
      isAuthenticated: false,
      isBootstrapping: true,
      redirectPath: "/login",
    });

    expect(screen.getByText("Checking access...")).toBeInTheDocument();
  });

  it("redirects guests to the login page", () => {
    renderProtectedRoute({
      auth: null,
      isAuthenticated: false,
      isBootstrapping: false,
      redirectPath: "/login",
    });

    expect(screen.getByText("Login screen")).toBeInTheDocument();
  });

  it("redirects authenticated users with the wrong role", () => {
    renderProtectedRoute({
      auth: { role: "Employee" },
      isAuthenticated: true,
      isBootstrapping: false,
      redirectPath: "/employee/tasks",
    });

    expect(screen.getByText("Employee home")).toBeInTheDocument();
  });

  it("renders children for an allowed role", () => {
    renderProtectedRoute({
      auth: { role: "Manager" },
      isAuthenticated: true,
      isBootstrapping: false,
      redirectPath: "/manager/dashboard",
    });

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });
});
