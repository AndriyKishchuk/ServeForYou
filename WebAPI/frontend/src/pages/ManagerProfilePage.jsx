import { useEffect, useState } from "react";
import { extractErrorMessage, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

export default function ManagerProfilePage() {
  const { auth, logout } = useAuth();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let mounted = true;

    async function loadProfile() {
      try {
        const [{ data: me }, { data: managers }] = await Promise.all([
          userApi.getMe(),
          userApi.getManagers(),
        ]);

        const managerProfile = managers.find((item) => item.id === me.id);
        if (mounted) {
          setProfile({
            ...me,
            specialization: managerProfile?.specialization || "Service Coordination",
            rating: managerProfile?.rating || 4.8,
          });
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load profile."));
        }
      }
    }

    loadProfile();

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Manager Profile"
      subtitle="Your professional card inside ServeForYou."
      actions={<div className="workspace-badge">Manager profile</div>}
    >
      <section className="single-page-grid">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Basic information</h2>
              <p>This page replaces the old role badge at the bottom of the layout.</p>
            </div>
          </div>

          {error ? <div className="form-error">{error}</div> : null}

          {profile ? (
            <div className="profile-grid">
              <div className="profile-card-large">
                <span>Full name</span>
                <strong>
                  {profile.name} {profile.surname}
                </strong>
              </div>
              <div className="profile-card-large">
                <span>Email</span>
                <strong>{profile.email}</strong>
              </div>
              <div className="profile-card-large">
                <span>Specialization</span>
                <strong>{profile.specialization}</strong>
              </div>
              <div className="profile-card-large">
                <span>Rating</span>
                <strong>{profile.rating} / 5.0</strong>
              </div>
            </div>
          ) : (
            <div className="empty-inline">Loading profile...</div>
          )}
        </div>
      </section>
    </WorkspaceLayout>
  );
}
