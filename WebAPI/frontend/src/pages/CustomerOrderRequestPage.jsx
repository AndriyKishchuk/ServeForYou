import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { extractErrorMessage, taskApi, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

const initialForm = {
  taskName: "",
  taskDescription: "",
  managerUserId: "",
};

export default function CustomerOrderRequestPage() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [managers, setManagers] = useState([]);
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let mounted = true;

    async function loadManagers() {
      try {
        const { data } = await userApi.getManagers();
        if (!mounted) return;

        setManagers(data);

        const queryManagerId = searchParams.get("managerId");
        const defaultManagerId = queryManagerId && data.some((item) => String(item.id) === queryManagerId)
          ? queryManagerId
          : data[0]?.id
            ? String(data[0].id)
            : "";

        setForm((current) => ({ ...current, managerUserId: current.managerUserId || defaultManagerId }));
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
  }, [searchParams]);

  const selectedManager = useMemo(
    () => managers.find((manager) => String(manager.id) === String(form.managerUserId)),
    [managers, form.managerUserId]
  );

  const handleChange = ({ target }) => {
    setForm((current) => ({ ...current, [target.name]: target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");
    setSubmitting(true);

    try {
      await taskApi.createCustomerRequest({
        taskName: form.taskName,
        taskDescription: form.taskDescription,
        managerUserId: Number(form.managerUserId),
      });

      setSuccess("Request sent to manager.");
      setTimeout(() => navigate("/customer/overview"), 500);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not create request."));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Request Work"
      subtitle="Write the brief and send it directly to a chosen manager."
      actions={
        <Link className="secondary-action" to="/customer/managers">
          Back to managers
        </Link>
      }
    >
      <section className="single-page-grid">
        <div className="panel-surface form-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Request details</h2>
              <p>Managers receive your request first and then forward it to the right employee.</p>
            </div>
          </div>

          <form className="editor-form" onSubmit={handleSubmit}>
            <label>
              Manager
              <select name="managerUserId" value={form.managerUserId} onChange={handleChange} required>
                <option value="">Select manager</option>
                {managers.map((manager) => (
                  <option key={manager.id} value={manager.id}>
                    {manager.name} {manager.surname} - {manager.specialization}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Request title
              <input
                name="taskName"
                value={form.taskName}
                onChange={handleChange}
                placeholder="Website content refresh"
                required
              />
            </label>

            <label>
              Description
              <textarea
                name="taskDescription"
                value={form.taskDescription}
                onChange={handleChange}
                rows={9}
                placeholder="Describe the work you want to order, expected result, deadline and notes."
                required
              />
            </label>

            {error ? <div className="form-error">{error}</div> : null}
            {success ? <div className="form-success">{success}</div> : null}

            <div className="form-actions-row">
              <button className="primary-action" type="submit" disabled={submitting}>
                {submitting ? "Sending..." : "Send to manager"}
              </button>
            </div>
          </form>
        </div>

        <aside className="panel-surface side-note">
          <div className="panel-surface-head">
            <div>
              <h2>Selected manager</h2>
              <p>Manager choice happens before task execution now.</p>
            </div>
          </div>

          {selectedManager ? (
            <div className="manager-profile-card">
              <div className="manager-profile-top">
                <span className="workspace-badge">{selectedManager.rating}/5</span>
                <strong>
                  {selectedManager.name} {selectedManager.surname}
                </strong>
              </div>
              <p>{selectedManager.specialization}</p>
              <span>{selectedManager.email}</span>
              <small>{selectedManager.companyName}</small>
            </div>
          ) : (
            <div className="empty-inline">Select a manager to continue.</div>
          )}
        </aside>
      </section>
    </WorkspaceLayout>
  );
}
