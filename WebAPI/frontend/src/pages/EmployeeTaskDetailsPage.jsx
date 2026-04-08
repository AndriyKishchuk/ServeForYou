import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { extractErrorMessage, fileApi, taskApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";

function downloadBlob(blob, fileName) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  window.URL.revokeObjectURL(url);
}

export default function EmployeeTaskDetailsPage() {
  const { auth, logout } = useAuth();
  const { taskId } = useParams();
  const [task, setTask] = useState(null);
  const [files, setFiles] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);

  const attachments = useMemo(
    () => files.filter((file) => file.category === "Attachment"),
    [files]
  );
  const results = useMemo(() => files.filter((file) => file.category === "Result"), [files]);
  const canUploadResult = task && task.status !== "SubmittedToManager" && task.status !== "ReturnedToAdmin";

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
            setError(extractErrorMessage(fileError, "Could not load task files."));
          }
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load task details."));
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

  const handleStatusUpdate = async (status) => {
    setError("");
    setSuccess("");

    try {
      const { data } = await taskApi.updateStatus(taskId, status);
      setTask((current) => ({ ...current, ...data }));
      setSuccess(`Status updated to ${status}.`);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not update task status."));
    }
  };

  const handleUpload = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setError("");
    setSuccess("");

    try {
      await fileApi.upload(taskId, file, "Result");
      const { data } = await fileApi.getByTask(taskId);
      setFiles(data);
      setSuccess("Result file uploaded.");
    } catch (err) {
      setError(extractErrorMessage(err, "Could not upload result file."));
    } finally {
      setUploading(false);
      event.target.value = "";
    }
  };

  const handleDownload = async (file) => {
    try {
      const response = await fileApi.download(file.id);
      downloadBlob(response.data, file.fileName);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not download file."));
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title={task ? task.taskName : "Task details"}
      subtitle="Your working space for status changes, downloads and result uploads."
      actions={
        <Link className="secondary-action" to="/employee/tasks">
          Back to my tasks
        </Link>
      }
    >
      {loading ? (
        <div className="empty-block">Loading task details...</div>
      ) : task ? (
        <section className="details-page-grid">
          <div className="panel-surface details-primary">
            <div className="panel-surface-head">
              <div>
                <h2>Task brief</h2>
                <p>{task.description}</p>
              </div>
              <span className={`task-status large ${task.status.toLowerCase()}`}>{task.status}</span>
            </div>

            <div className="details-metadata">
              <div>
                <span>Manager</span>
                <strong>
                  {task.managerUser?.name} {task.managerUser?.surname}
                </strong>
              </div>
              <div>
                <span>Status control</span>
                <div className="inline-actions">
                  {task.status === "New" ? (
                    <button className="primary-action" onClick={() => handleStatusUpdate("InProgress")}>
                      Start work
                    </button>
                  ) : null}
                  {task.status === "InProgress" ? (
                    <button className="primary-action" onClick={() => handleStatusUpdate("SubmittedToManager")}>
                      Send to manager
                    </button>
                  ) : null}
                </div>
              </div>
            </div>

            {error ? <div className="form-error">{error}</div> : null}
            {success ? <div className="form-success">{success}</div> : null}

            <div className="files-section">
              <div className="files-section-head">
                <div>
                  <h3>Manager attachments</h3>
                  <p>Download the materials required to complete the task.</p>
                </div>
              </div>
              <div className="file-stack">
                {attachments.length ? (
                  attachments.map((file) => (
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
                  <div className="empty-inline">No attachments available yet.</div>
                )}
              </div>
            </div>
          </div>

          <aside className="panel-stack">
            <section className="panel-surface">
              <div className="panel-surface-head">
                <div>
                  <h2>Upload result</h2>
                  <p>Attach files before you submit the completed work back to the manager.</p>
                </div>
              </div>
              <label className={uploading || !canUploadResult ? "primary-action disabled-action" : "primary-action"}>
                <input type="file" onChange={handleUpload} disabled={uploading || !canUploadResult} />
                {uploading ? "Uploading..." : "Choose file"}
              </label>
            </section>

            <section className="panel-surface">
              <div className="panel-surface-head">
                <div>
                  <h2>Uploaded results</h2>
                  <p>Your submitted files appear here.</p>
                </div>
              </div>
              <div className="file-stack">
                {results.length ? (
                  results.map((file) => (
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
                  <div className="empty-inline">No result files uploaded yet.</div>
                )}
              </div>
            </section>
          </aside>
        </section>
      ) : (
        <div className="empty-block">Task not found.</div>
      )}
    </WorkspaceLayout>
  );
}
