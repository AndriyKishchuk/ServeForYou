import { useEffect, useState } from "react";
import { extractErrorMessage, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

export default function EmployeeProfilePage() {
  const { auth, logout } = useAuth();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let mounted = true;

    async function loadProfile() {
      try {
        const { data } = await userApi.getMe();
        if (mounted) {
          setProfile(data);
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
      title="Employee Profile"
      subtitle="Your basic workspace identity and account details."
      actions={<div className="workspace-badge">Employee profile</div>}
    >
      <section className="single-page-grid">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Basic information</h2>
              <p>A dedicated page for employee information instead of showing the role in the sidebar footer.</p>
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
                <span>Role</span>
                <strong>{profile.role}</strong>
              </div>
              <div className="profile-card-large">
                <span>Company</span>
                <strong>{profile.company?.companyName || "ServeForYou"}</strong>
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
