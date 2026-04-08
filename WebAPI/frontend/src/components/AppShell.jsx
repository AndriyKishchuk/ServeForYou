import { Link, useLocation } from "react-router-dom";

export default function AppShell({
  title,
  eyebrow,
  description,
  metrics,
  actions,
  children,
  user,
  onLogout,
}) {
  const location = useLocation();

  const links = [
    { to: "/manager/dashboard", label: "Manager", roles: ["Manager"] },
    { to: "/employee/tasks", label: "Employee", roles: ["Employee"] },
    { to: "/admin/dashboard", label: "Admin", roles: ["Admin"] },
  ].filter((item) => item.roles.includes(user?.role));

  return (
    <div className="app-shell">
      <aside className="shell-sidebar">
        <div>
          <div className="brand-mark">TF</div>
          <p className="brand-eyebrow">TaskFlow Service</p>
          <h1 className="brand-title">Workspaces that actually feel alive.</h1>
          <p className="brand-copy">
            Assign, execute and deliver work with one clean flow for managers and employees.
          </p>
        </div>

        <nav className="shell-nav">
          {links.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className={location.pathname === link.to ? "shell-link active" : "shell-link"}
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="profile-card">
          <span className="profile-role">{user?.role}</span>
          <strong>{user?.name}</strong>
          <span>{user?.email}</span>
          <span>Company #{user?.companyId}</span>
          <button className="ghost-button" onClick={onLogout}>
            Sign out
          </button>
        </div>
      </aside>

      <main className="shell-main">
        <section className="hero-panel">
          <div>
            <span className="hero-eyebrow">{eyebrow}</span>
            <h2>{title}</h2>
            <p>{description}</p>
          </div>
          <div className="hero-actions">{actions}</div>
        </section>

        {metrics?.length ? (
          <section className="metrics-grid">
            {metrics.map((metric) => (
              <article key={metric.label} className="metric-card">
                <span>{metric.label}</span>
                <strong>{metric.value}</strong>
                <small>{metric.caption}</small>
              </article>
            ))}
          </section>
        ) : null}

        <section className="workspace-grid">{children}</section>
      </main>
    </div>
  );
}
