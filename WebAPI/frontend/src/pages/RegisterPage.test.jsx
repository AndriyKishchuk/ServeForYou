import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import RegisterPage from "./RegisterPage";
import { useAuth } from "../context/AuthContext";

const mockNavigate = jest.fn();

jest.mock("../context/AuthContext", () => ({
  useAuth: jest.fn(),
}));

jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockNavigate,
}));

describe("RegisterPage", () => {
  beforeEach(() => {
    mockNavigate.mockReset();
    useAuth.mockReturnValue({
      register: jest.fn(),
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it("submits registration data and redirects by role", async () => {
    const register = jest.fn().mockResolvedValue({ redirectUrl: "/customer/overview" });
    useAuth.mockReturnValue({ register });

    render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText("First name"), { target: { value: "Andrii" } });
    fireEvent.change(screen.getByLabelText("Last name"), { target: { value: "Koval" } });
    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "admin@test.com" } });
    fireEvent.change(screen.getByLabelText("Password"), { target: { value: "StrongPass123" } });
    fireEvent.change(screen.getByLabelText("Role"), { target: { value: "Admin" } });
    fireEvent.click(screen.getByRole("button", { name: "Register and continue" }));

    await waitFor(() => {
      expect(register).toHaveBeenCalledWith({
        name: "Andrii",
        surname: "Koval",
        email: "admin@test.com",
        password: "StrongPass123",
        role: "Admin",
      });
    });

    expect(mockNavigate).toHaveBeenCalledWith("/customer/overview", { replace: true });
  });

  it("shows an error when registration fails", async () => {
    const register = jest.fn().mockRejectedValue({
      response: { data: { message: "Registration failed." } },
    });
    useAuth.mockReturnValue({ register });

    render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText("First name"), { target: { value: "Andrii" } });
    fireEvent.change(screen.getByLabelText("Last name"), { target: { value: "Koval" } });
    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "admin@test.com" } });
    fireEvent.change(screen.getByLabelText("Password"), { target: { value: "StrongPass123" } });
    fireEvent.click(screen.getByRole("button", { name: "Register and continue" }));

    expect(await screen.findByText("Registration failed.")).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
