import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import LoginPage from "./LoginPage";
import { useAuth } from "../context/AuthContext";

const mockNavigate = jest.fn();

jest.mock("../context/AuthContext", () => ({
  useAuth: jest.fn(),
}));

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockNavigate,
}));

describe("LoginPage", () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    useAuth.mockReturnValue({
      login: jest.fn(),
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("submits credentials and redirects to the returned page", async () => {
    const login = jest.fn().mockResolvedValue({ redirectUrl: "/manager/dashboard" });
    useAuth.mockReturnValue({ login });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "manager@test.com" } });
    fireEvent.change(screen.getByLabelText("Password"), { target: { value: "pass123" } });
    fireEvent.click(screen.getByRole("button", { name: "Enter dashboard" }));

    await waitFor(() => {
      expect(login).toHaveBeenCalledWith({
        email: "manager@test.com",
        password: "pass123",
      });
    });

    expect(mockNavigate).toHaveBeenCalledWith("/manager/dashboard", { replace: true });
  });

  it("renders backend errors when sign in fails", async () => {
    const login = jest.fn().mockRejectedValue({
      response: { data: { message: "Invalid email or password." } },
    });
    useAuth.mockReturnValue({ login });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "manager@test.com" } });
    fireEvent.change(screen.getByLabelText("Password"), { target: { value: "wrongpass" } });
    fireEvent.click(screen.getByRole("button", { name: "Enter dashboard" }));

    expect(await screen.findByText("Invalid email or password.")).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
