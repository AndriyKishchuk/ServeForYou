import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { extractErrorMessage, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

function roleCopy(role) {
  switch (role) {
    case "Manager":
      return "Manager account settings";
    case "Employee":
      return "Employee account settings";
    default:
      return "Customer account settings";
  }
}

export default function SettingsPage() {
  const { auth, logout, profile, refreshProfile } = useAuth();
  const navigate = useNavigate();
  const [profileForm, setProfileForm] = useState({
    name: "",
    surname: "",
    email: "",
  });
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: "",
    newPassword: "",
  });
  const [profileError, setProfileError] = useState("");
  const [profileSuccess, setProfileSuccess] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [deleteError, setDeleteError] = useState("");
  const [deleteSuccess, setDeleteSuccess] = useState("");
  const [isSavingProfile, setIsSavingProfile] = useState(false);
  const [isSavingPassword, setIsSavingPassword] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    setProfileForm({
      name: profile?.name || auth?.name || "",
      surname: profile?.surname || "",
      email: profile?.email || auth?.email || "",
    });
  }, [auth?.email, auth?.name, profile?.email, profile?.name, profile?.surname]);

  const handleProfileChange = (event) => {
    const { name, value } = event.target;
    setProfileForm((current) => ({ ...current, [name]: value }));
  };

  const handlePasswordChange = (event) => {
    const { name, value } = event.target;
    setPasswordForm((current) => ({ ...current, [name]: value }));
  };

  const handleProfileSubmit = async (event) => {
    event.preventDefault();
    setProfileError("");
    setProfileSuccess("");
    setIsSavingProfile(true);

    try {
      await userApi.updateMe(profileForm);
      await refreshProfile();
      setProfileSuccess("Profile updated successfully.");
    } catch (err) {
      setProfileError(extractErrorMessage(err, "Could not update your profile."));
    } finally {
      setIsSavingProfile(false);
    }
  };

  const handlePasswordSubmit = async (event) => {
    event.preventDefault();
    setPasswordError("");
    setPasswordSuccess("");
    setIsSavingPassword(true);

    try {
      await userApi.changeMyPassword(passwordForm);
      setPasswordForm({ currentPassword: "", newPassword: "" });
      setPasswordSuccess("Password changed successfully.");
    } catch (err) {
      setPasswordError(extractErrorMessage(err, "Could not change your password."));
    } finally {
      setIsSavingPassword(false);
    }
  };

  const handleDeleteAccount = async () => {
    setDeleteError("");
    setDeleteSuccess("");

    const confirmed = window.confirm(
      "Delete your account? This action cannot be undone."
    );

    if (!confirmed) {
      return;
    }

    setIsDeleting(true);

    try {
      await userApi.deleteMe();
      setDeleteSuccess("Your account has been deleted.");
      logout();
      navigate("/login", { replace: true });
    } catch (err) {
      setDeleteError(extractErrorMessage(err, "Could not delete your account."));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Settings"
      subtitle="Manage your account access and review destructive actions in one place."
    >
      <section className="single-page-grid">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>{roleCopy(auth?.role)}</h2>
              <p>
                This area is for account-level actions. More preferences can be added here later when you redesign the
                rest of the workspace.
              </p>
            </div>
          </div>

          <div className="profile-grid">
            <div className="profile-card-large">
              <span>Name</span>
              <strong>{auth?.name || "Unknown user"}</strong>
            </div>
            <div className="profile-card-large">
              <span>Email</span>
              <strong>{auth?.email || "No email"}</strong>
            </div>
            <div className="profile-card-large">
              <span>Role</span>
              <strong>{auth?.role || "No role"}</strong>
            </div>
            <div className="profile-card-large">
              <span>Workspace</span>
              <strong>ServeForYou</strong>
            </div>
          </div>

          <form className="editor-form settings-form" onSubmit={handleProfileSubmit}>
            <div className="panel-surface-head compact">
              <div>
                <h2>Edit profile</h2>
                <p>Update the identity details shown across your workspace.</p>
              </div>
            </div>

            {profileError ? <div className="form-error">{profileError}</div> : null}
            {profileSuccess ? <div className="form-success">{profileSuccess}</div> : null}

            <div className="form-grid two-columns">
              <label>
                First name
                <input name="name" value={profileForm.name} onChange={handleProfileChange} required />
              </label>
              <label>
                Last name
                <input name="surname" value={profileForm.surname} onChange={handleProfileChange} required />
              </label>
            </div>

            <label>
              Email
              <input type="email" name="email" value={profileForm.email} onChange={handleProfileChange} required />
            </label>

            <div className="form-actions-row">
              <button className="primary-action" type="submit" disabled={isSavingProfile}>
                {isSavingProfile ? "Saving..." : "Save profile"}
              </button>
            </div>
          </form>
        </div>

        <aside className="panel-stack">
          <section className="panel-surface">
            <div className="panel-surface-head">
              <div>
                <h2>Change password</h2>
                <p>Keep account access secure without leaving the workspace.</p>
              </div>
            </div>

            <form className="editor-form settings-form" onSubmit={handlePasswordSubmit}>
              {passwordError ? <div className="form-error">{passwordError}</div> : null}
              {passwordSuccess ? <div className="form-success">{passwordSuccess}</div> : null}

              <label>
                Current password
                <input
                  type="password"
                  name="currentPassword"
                  value={passwordForm.currentPassword}
                  onChange={handlePasswordChange}
                  required
                />
              </label>

              <label>
                New password
                <input
                  type="password"
                  name="newPassword"
                  value={passwordForm.newPassword}
                  onChange={handlePasswordChange}
                  minLength={6}
                  required
                />
              </label>

              <div className="form-actions-row">
                <button className="primary-action" type="submit" disabled={isSavingPassword}>
                  {isSavingPassword ? "Updating..." : "Update password"}
                </button>
              </div>
            </form>
          </section>

          <section className="panel-surface danger-panel">
            <div className="panel-surface-head">
              <div>
                <h2>Delete my account</h2>
                <p>
                  Use this only if you want to permanently remove your access. If your account is linked to tasks or
                  uploaded files, the backend will block deletion.
                </p>
              </div>
            </div>

            {deleteError ? <div className="form-error">{deleteError}</div> : null}
            {deleteSuccess ? <div className="form-success">{deleteSuccess}</div> : null}

            <div className="timeline-list">
              <div>
                <strong>Safe guard</strong>
                <span>Accounts with related tasks or files are protected from accidental deletion.</span>
              </div>
              <div>
                <strong>Permanent action</strong>
                <span>Once deletion succeeds, you are signed out immediately.</span>
              </div>
            </div>

            <div className="form-actions-row">
              <button className="ghost-action danger" onClick={handleDeleteAccount} disabled={isDeleting}>
                {isDeleting ? "Deleting..." : "Delete my account"}
              </button>
            </div>
          </section>
        </aside>
      </section>
    </WorkspaceLayout>
  );
}
