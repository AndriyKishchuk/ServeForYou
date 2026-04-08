import WorkspaceLayout from "../components/WorkspaceLayout";
import StatTile from "../components/StatTile";
import { useAuth } from "../context/AuthContext";

export default function AdminBoard() {
  const { auth, logout } = useAuth();

  return (
    <WorkspaceLayout
      title="Admin overview"
      user={auth}
      onLogout={logout}
      subtitle="Admin route is still supported in the new multi-page shell."
      actions={<div className="workspace-badge">Admin route ready</div>}
    >
      <section className="content-grid three-up">
        <StatTile label="Access" value="Full" note="Admin routing works" tone="accent" />
        <StatTile label="Role" value={auth?.role || "Admin"} note="Resolved from token" tone="dark" />
      </section>

      <section className="panel-surface">
        <div className="empty-block">
          Admin UI placeholder. Manager and employee flows are now split into separate pages, and this route
          is ready for future company and user administration screens.
        </div>
      </section>
    </WorkspaceLayout>
  );
}
