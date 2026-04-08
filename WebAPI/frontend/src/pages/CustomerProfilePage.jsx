import { useEffect, useState } from "react";
import { extractErrorMessage, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

export default function CustomerProfilePage() {
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
          setError(extractErrorMessage(err, "Could not load customer profile."));
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
      title="Customer Profile"
      subtitle="Basic account information for the customer side of the system."
      actions={<div className="workspace-badge">Client account</div>}
    >
      <section className="single-page-grid">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Basic information</h2>
              <p>The Admin role is now represented as the customer area of ServeForYou.</p>
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
                <span>Account type</span>
                <strong>Customer</strong>
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
