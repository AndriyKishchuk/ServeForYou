import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthProvider, useAuth } from "./AuthContext";
import {
  authApi,
  clearStoredAuth,
  getStoredAuth,
  setStoredAuth,
  userApi,
} from "../api/api";

jest.mock("../api/api", () => ({
  authApi: {
    me: jest.fn(),
    login: jest.fn(),
    register: jest.fn(),
  },
  userApi: {
    getMe: jest.fn(),
  },
  getStoredAuth: jest.fn(),
  setStoredAuth: jest.fn(),
  clearStoredAuth: jest.fn(),
}));

function AuthConsumer() {
  const {
    auth,
    profile,
    isAuthenticated,
    isBootstrapping,
    redirectPath,
    login,
    logout,
  } = useAuth();

  return (
    <div>
      <div data-testid="bootstrapping">{String(isBootstrapping)}</div>
      <div data-testid="authenticated">{String(isAuthenticated)}</div>
      <div data-testid="role">{auth?.role || "none"}</div>
      <div data-testid="profile-name">{profile?.name || "none"}</div>
      <div data-testid="redirect-path">{redirectPath}</div>
      <button onClick={() => login({ email: "manager@test.com", password: "pass123" })}>Login</button>
      <button onClick={logout}>Logout</button>
    </div>
  );
}

describe("AuthContext", () => {
  beforeEach(() => {
    getStoredAuth.mockReturnValue(null);
    authApi.me.mockReset();
    authApi.login.mockReset();
    authApi.register.mockReset();
    userApi.getMe.mockReset();
    setStoredAuth.mockReset();
    clearStoredAuth.mockReset();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("finishes bootstrapping immediately when there is no stored token", async () => {
    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("bootstrapping")).toHaveTextContent("false");
    });

    expect(screen.getByTestId("authenticated")).toHaveTextContent("false");
    expect(screen.getByTestId("redirect-path")).toHaveTextContent("/login");
  });

  it("restores auth and profile from stored token during bootstrap", async () => {
    getStoredAuth.mockReturnValue({ token: "abc", role: "Manager", redirectUrl: "/manager/dashboard" });
    authApi.me.mockResolvedValue({ data: { role: "Manager", redirectUrl: "/manager/dashboard" } });
    userApi.getMe.mockResolvedValue({ data: { name: "Olena" } });

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("profile-name")).toHaveTextContent("Olena");
    });

    expect(screen.getByTestId("authenticated")).toHaveTextContent("true");
    expect(screen.getByTestId("role")).toHaveTextContent("Manager");
    expect(setStoredAuth).toHaveBeenCalled();
  });

  it("logs in, stores auth and loads the profile", async () => {
    authApi.login.mockResolvedValue({
      data: { token: "jwt-token", role: "Manager", redirectUrl: "/manager/dashboard" },
    });
    userApi.getMe.mockResolvedValue({ data: { name: "Taras" } });

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await act(async () => {
      await userEvent.click(screen.getByRole("button", { name: "Login" }));
    });

    await waitFor(() => {
      expect(screen.getByTestId("authenticated")).toHaveTextContent("true");
    });

    expect(authApi.login).toHaveBeenCalledWith({ email: "manager@test.com", password: "pass123" });
    expect(setStoredAuth).toHaveBeenCalledWith({
      token: "jwt-token",
      role: "Manager",
      redirectUrl: "/manager/dashboard",
    });
    expect(screen.getByTestId("profile-name")).toHaveTextContent("Taras");
  });

  it("clears auth state on logout", async () => {
    authApi.login.mockResolvedValue({
      data: { token: "jwt-token", role: "Manager", redirectUrl: "/manager/dashboard" },
    });
    userApi.getMe.mockResolvedValue({ data: { name: "Taras" } });

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await act(async () => {
      await userEvent.click(screen.getByRole("button", { name: "Login" }));
    });

    await waitFor(() => {
      expect(screen.getByTestId("authenticated")).toHaveTextContent("true");
    });

    await act(async () => {
      await userEvent.click(screen.getByRole("button", { name: "Logout" }));
    });

    expect(clearStoredAuth).toHaveBeenCalled();
    expect(screen.getByTestId("authenticated")).toHaveTextContent("false");
    expect(screen.getByTestId("profile-name")).toHaveTextContent("none");
  });
});
