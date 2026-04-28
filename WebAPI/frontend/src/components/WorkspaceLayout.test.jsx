import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import WorkspaceLayout from "./WorkspaceLayout";

describe("WorkspaceLayout", () => {
  it("renders manager navigation and shared account actions", () => {
    render(
      <MemoryRouter initialEntries={["/manager/dashboard"]}>
        <WorkspaceLayout
          user={{ role: "Manager", name: "Iryna", email: "manager@test.com" }}
          title="Dashboard"
          subtitle="Track work"
          actions={<button>Create</button>}
          onLogout={() => {}}
        >
          <div>Manager content</div>
        </WorkspaceLayout>
      </MemoryRouter>
    );

    expect(screen.getByText("Overview")).toBeInTheDocument();
    expect(screen.getByText("Create Task")).toBeInTheDocument();
    expect(screen.getByText("My Profile")).toBeInTheDocument();
    expect(screen.getByText("Settings")).toBeInTheDocument();
    expect(screen.getByText("Iryna")).toBeInTheDocument();
    expect(screen.getByText("manager@test.com")).toBeInTheDocument();
    expect(screen.getByText("Manager content")).toBeInTheDocument();
  });

  it("renders admin navigation links", () => {
    render(
      <MemoryRouter initialEntries={["/customer/overview"]}>
        <WorkspaceLayout
          user={{ role: "Admin", name: "Anna", email: "admin@test.com" }}
          title="Overview"
          subtitle="Customer space"
          onLogout={() => {}}
        >
          <div>Admin content</div>
        </WorkspaceLayout>
      </MemoryRouter>
    );

    expect(screen.getByText("Find Manager")).toBeInTheDocument();
    expect(screen.getByText("Request Work")).toBeInTheDocument();
    expect(screen.getByText("Admin content")).toBeInTheDocument();
  });
});
