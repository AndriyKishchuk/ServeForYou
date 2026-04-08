import { Link, NavLink, useLocation } from "react-router-dom";

function getNavigation(role) {
  if (role === "Manager") {
    return [
      { to: "/manager/dashboard", label: "Overview" },
      { to: "/manager/tasks/new", label: "Create Task" },
      { to: "/manager/profile", label: "My Profile" },
    ];
  }

  if (role === "Employee") {
    return [
      { to: "/employee/tasks", label: "My Tasks" },
      { to: "/employee/profile", label: "My Profile" },
    ];
  }

  return [
    { to: "/customer/overview", label: "Overview" },
    { to: "/customer/managers", label: "Find Manager" },
    { to: "/customer/orders/new", label: "Request Work" },
    { to: "/customer/profile", label: "My Profile" },
  ];
}

function SettingsIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" className="settings-icon">
      <path
        d="M12 8.75A3.25 3.25 0 1 0 12 15.25A3.25 3.25 0 1 0 12 8.75Z"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.7"
      />
      <path
        d="M19.03 12.88a7.97 7.97 0 0 0 .05-.88c0-.3-.02-.59-.05-.88l1.93-1.5a.6.6 0 0 0 .14-.76l-1.83-3.18a.6.6 0 0 0-.72-.27l-2.28.92a7.62 7.62 0 0 0-1.52-.88l-.35-2.43A.59.59 0 0 0 13.82 2h-3.64a.59.59 0 0 0-.59.5l-.35 2.43c-.54.21-1.05.5-1.52.88l-2.28-.92a.6.6 0 0 0-.72.27L2.89 8.34a.6.6 0 0 0 .14.76l1.93 1.5c-.03.29-.05.58-.05.88s.02.59.05.88l-1.93 1.5a.6.6 0 0 0-.14.76l1.83 3.18c.15.26.46.37.72.27l2.28-.92c.47.38.98.67 1.52.88l.35 2.43c.05.29.29.5.59.5h3.64c.3 0 .54-.21.59-.5l.35-2.43c.54-.21 1.05-.5 1.52-.88l2.28.92c.26.1.57-.01.72-.27l1.83-3.18a.6.6 0 0 0-.14-.76l-1.93-1.5Z"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export default function WorkspaceLayout({
  user,
  title,
  subtitle,
  actions,
  children,
  onLogout,
}) {
  const location = useLocation();
  const navigation = getNavigation(user?.role);

  return (
    <div className="workspace-shell">
      <aside className="workspace-sidebar">
        <Link className="workspace-logo" to="/">
          <span className="workspace-logo-mark">TF</span>
          <div>
            <strong>ServeForYou</strong>
            <span>Service workspace</span>
          </div>
        </Link>

        <nav className="workspace-nav">
          {navigation.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                isActive || location.pathname.startsWith(`${item.to}/`)
                  ? "workspace-nav-link active"
                  : "workspace-nav-link"
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="workspace-sidebar-card">
          <Link className="settings-entry" to="/settings">
            <SettingsIcon />
            <span>Settings</span>
          </Link>
          <strong>{user?.name}</strong>
          <span>{user?.email}</span>
          <button className="ghost-action" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </aside>

      <div className="workspace-main">
        <header className="workspace-topbar">
          <div>
            <h1>{title}</h1>
            <p>{subtitle}</p>
          </div>
          <div className="workspace-topbar-actions">{actions}</div>
        </header>

        <main className="workspace-content">{children}</main>
      </div>
    </div>
  );
}
