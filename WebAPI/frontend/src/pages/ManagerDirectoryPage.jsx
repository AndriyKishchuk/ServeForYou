import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { extractErrorMessage, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

export default function ManagerDirectoryPage() {
  const { auth, logout } = useAuth();
  const [managers, setManagers] = useState([]);
  const [query, setQuery] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    let mounted = true;

    async function loadManagers() {
      try {
        const { data } = await userApi.getManagers();
        if (mounted) {
          setManagers(data);
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load managers."));
        }
      }
    }

    loadManagers();

    return () => {
      mounted = false;
    };
  }, []);

  const filteredManagers = useMemo(() => {
    const value = query.trim().toLowerCase();
    if (!value) return managers;

    return managers.filter((manager) =>
      `${manager.name} ${manager.surname} ${manager.specialization}`.toLowerCase().includes(value)
    );
  }, [managers, query]);

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Find a Manager"
      subtitle="Search by name or specialization, compare ratings, and choose who should receive your request."
      actions={<div className="workspace-badge">Manager directory</div>}
    >
      <section className="panel-surface">
        <div className="panel-surface-head">
          <div>
            <h2>Search managers</h2>
            <p>This page shows rating and specialization for every manager in ServeForYou.</p>
          </div>
        </div>

        <input
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Search by manager name or specialization"
        />

        {error ? <div className="form-error">{error}</div> : null}

        <div className="manager-directory-grid">
          {filteredManagers.length ? (
            filteredManagers.map((manager) => (
              <article key={manager.id} className="manager-profile-card">
                <div className="manager-profile-top">
                  <span className="workspace-badge">{manager.rating}/5</span>
                  <strong>
                    {manager.name} {manager.surname}
                  </strong>
                </div>
                <p>{manager.specialization}</p>
                <span>{manager.email}</span>
                <small>{manager.companyName}</small>
                <Link className="primary-action" to={`/customer/orders/new?managerId=${manager.id}`}>
                  Order from this manager
                </Link>
              </article>
            ))
          ) : (
            <div className="empty-block">No managers match your search.</div>
          )}
        </div>
      </section>
    </WorkspaceLayout>
  );
}
