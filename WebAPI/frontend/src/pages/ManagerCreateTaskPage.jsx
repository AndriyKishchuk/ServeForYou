import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { taskApi, userApi, extractErrorMessage } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

const initialForm = {
  taskName: "",
  taskDescription: "",
  assignedToUserId: "",
};

export default function ManagerCreateTaskPage() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [employees, setEmployees] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loadingEmployees, setLoadingEmployees] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let mounted = true;

    async function loadEmployees() {
      try {
        const { data } = await userApi.getCompanyEmployees();
        if (mounted) {
          setEmployees(data);
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load employees."));
        }
      } finally {
        if (mounted) {
          setLoadingEmployees(false);
        }
      }
    }

    loadEmployees();

    return () => {
      mounted = false;
    };
  }, []);

  const handleChange = ({ target }) => {
    setForm((current) => ({ ...current, [target.name]: target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");
    setSubmitting(true);

    try {
      const { data } = await taskApi.create({
        taskName: form.taskName,
        taskDescription: form.taskDescription,
        assignedToUserId: Number(form.assignedToUserId),
      });

      setSuccess("Task created. Redirecting to details...");
      setTimeout(() => navigate(`/manager/tasks/${data.id}`), 500);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not create task."));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Create a Task"
      subtitle="A dedicated page for writing the brief and assigning the right employee."
      actions={<div className="workspace-badge">Manager workflow</div>}
    >
      <section className="single-page-grid">
        <div className="panel-surface form-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Task brief</h2>
              <p>Keep the assignment clear. Files can be uploaded from the task details page after creation.</p>
            </div>
          </div>

          <form className="editor-form" onSubmit={handleSubmit}>
            <label>
              Task title
              <input
                name="taskName"
                value={form.taskName}
                onChange={handleChange}
                placeholder="Prepare Q2 campaign report"
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
                placeholder="Describe the expected result, format, deadline and important notes."
                required
              />
            </label>

            <label>
              Assign employee
              <select
                name="assignedToUserId"
                value={form.assignedToUserId}
                onChange={handleChange}
                required
                disabled={loadingEmployees}
              >
                <option value="">Select employee</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.name} {employee.surname}
                  </option>
                ))}
              </select>
            </label>

            {error ? <div className="form-error">{error}</div> : null}
            {success ? <div className="form-success">{success}</div> : null}

            <div className="form-actions-row">
              <button className="primary-action" type="submit" disabled={submitting || !employees.length}>
                {submitting ? "Creating..." : "Create task"}
              </button>
            </div>
          </form>
        </div>

        <aside className="panel-surface side-note">
          <div className="panel-surface-head">
            <div>
              <h2>What happens next</h2>
              <p>The task opens on its own page after creation.</p>
            </div>
          </div>
          <div className="timeline-list">
            <div>
              <strong>1. Task is created</strong>
              <span>The selected employee becomes responsible for execution.</span>
            </div>
            <div>
              <strong>2. Manager adds files</strong>
              <span>Upload attachment files like briefs, PDFs or templates.</span>
            </div>
            <div>
              <strong>3. Employee works on it</strong>
              <span>Status changes and result files appear on the detail page.</span>
            </div>
          </div>
        </aside>
      </section>
    </WorkspaceLayout>
  );
}
