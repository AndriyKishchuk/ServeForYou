import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { extractErrorMessage, fileApi, taskApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";
import { getTaskStatusLabel } from "../utils/taskStatus";

function downloadBlob(blob, fileName) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  window.URL.revokeObjectURL(url);
}

export default function CustomerTaskDetailsPage() {
  const { auth, logout } = useAuth();
  const { taskId } = useParams();
  const [task, setTask] = useState(null);
  const [files, setFiles] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(true);

  const resultFiles = useMemo(() => files.filter((file) => file.category === "Result"), [files]);

  useEffect(() => {
    let mounted = true;

    async function loadTaskDetails() {
      try {
        const { data: taskData } = await taskApi.getById(taskId);
        if (!mounted) return;
        setTask(taskData);

        try {
          const { data: fileData } = await fileApi.getByTask(taskId);
          if (mounted) {
            setFiles(fileData);
          }
        } catch (fileError) {
          if (mounted) {
            setError(extractErrorMessage(fileError, "Could not load request files."));
          }
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load request details."));
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    }

    loadTaskDetails();

    return () => {
      mounted = false;
    };
  }, [taskId]);

  const handleDownload = async (file) => {
    try {
      const response = await fileApi.download(file.id);
      downloadBlob(response.data, file.fileName);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not download file."));
    }
  };

  const handleComplete = async () => {
    setError("");
    setSuccess("");

    try {
      const { data } = await taskApi.complete(taskId);
      setTask((current) => ({ ...current, ...data }));
      setSuccess("Task confirmed and marked as done.");
    } catch (err) {
      setError(extractErrorMessage(err, "Could not complete the task."));
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title={task ? task.taskName : "Request details"}
      subtitle="Final step of the flow: the manager returns completed work back to you."
      actions={
        <Link className="secondary-action" to="/customer/overview">
          Back to overview
        </Link>
      }
    >
      {loading ? (
        <div className="empty-block">Loading request details...</div>
      ) : task ? (
        <section className="details-page-grid">
          <div className="panel-surface details-primary">
            <div className="panel-surface-head">
              <div>
                <h2>Request summary</h2>
                <p>{task.description}</p>
              </div>
              <span className={`task-status large ${task.status.toLowerCase()}`}>
                {getTaskStatusLabel(task.status)}
              </span>
            </div>

            <div className="details-metadata">
              <div>
                <span>Manager</span>
                <strong>
                  {task.managerUser?.name} {task.managerUser?.surname}
                </strong>
              </div>
              <div>
                <span>Employee</span>
                <strong>
                  {task.assignedToByUser?.name} {task.assignedToByUser?.surname}
                </strong>
              </div>
            </div>

            {error ? <div className="form-error">{error}</div> : null}
            {success ? <div className="form-success">{success}</div> : null}

            <div className="timeline-list">
              <div>
                <strong>Admin -> Manager</strong>
                <span>You created the request and selected the manager.</span>
              </div>
              <div>
                <strong>Manager -> Employee</strong>
                <span>The manager delegated execution to the selected employee.</span>
              </div>
              <div>
                <strong>Employee -> Manager -> Admin</strong>
                <span>The completed result now returns to you through the manager.</span>
              </div>
            </div>

            {task.status === "ReturnedToAdmin" ? (
              <div className="form-actions-row">
                <button className="primary-action" onClick={handleComplete}>
                  Everything is okay
                </button>
              </div>
            ) : null}
          </div>

          <aside className="panel-surface">
            <div className="panel-surface-head">
              <div>
                <h2>Delivered files</h2>
                <p>
                  {task.status === "ReturnedToAdmin"
                    ? "Result files returned to the customer through the manager."
                    : "Files stay with the manager until the task is officially returned to you."}
                </p>
              </div>
            </div>

            <div className="file-stack">
              {task.status !== "ReturnedToAdmin" ? (
                <div className="empty-inline">The task is still being processed by the manager.</div>
              ) : resultFiles.length ? (
                resultFiles.map((file) => (
                  <div key={file.id} className="file-row">
                    <div>
                      <strong>{file.fileName}</strong>
                      <span>{file.uploadedByName}</span>
                    </div>
                    <button className="secondary-action" onClick={() => handleDownload(file)}>
                      Download
                    </button>
                  </div>
                ))
              ) : (
                <div className="empty-inline">No delivered files yet.</div>
              )}
            </div>
          </aside>
        </section>
      ) : (
        <div className="empty-block">Request not found.</div>
      )}
    </WorkspaceLayout>
  );
}
