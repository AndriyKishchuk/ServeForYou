import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { extractErrorMessage, fileApi, taskApi, userApi } from "../api/api";
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

export default function ManagerTaskDetailsPage() {
  const { auth, logout } = useAuth();
  const { taskId } = useParams();
  const navigate = useNavigate();
  const [task, setTask] = useState(null);
  const [files, setFiles] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [employees, setEmployees] = useState([]);
  const [selectedEmployeeId, setSelectedEmployeeId] = useState("");

  const attachments = useMemo(
    () => files.filter((file) => file.category === "Attachment"),
    [files]
  );
  const results = useMemo(() => files.filter((file) => file.category === "Result"), [files]);

  useEffect(() => {
    let mounted = true;

    async function loadTaskDetails() {
      try {
        const { data: taskData } = await taskApi.getById(taskId);
        const { data: employeeData } = await userApi.getCompanyEmployees();

        if (!mounted) return;
        setTask(taskData);
        setEmployees(employeeData);
        if (taskData.assignedToUserId && taskData.assignedToUserId !== taskData.managerUserId) {
          setSelectedEmployeeId(String(taskData.assignedToUserId));
        }

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

  const handleUpload = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setError("");
    setSuccess("");

    try {
      await fileApi.upload(taskId, file, "Attachment");
      const { data } = await fileApi.getByTask(taskId);
      setFiles(data);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not upload attachment."));
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

  const handleDelete = async () => {
    try {
      await taskApi.delete(taskId);
      navigate("/manager/dashboard");
    } catch (err) {
      setError(extractErrorMessage(err, "Could not delete task."));
    }
  };

  const handleAssign = async () => {
    if (!selectedEmployeeId) return;

    try {
      await taskApi.assignToEmployee(taskId, Number(selectedEmployeeId));
      const { data } = await taskApi.getById(taskId);
      setTask(data);
      setSuccess("Task forwarded to employee.");
    } catch (err) {
      setError(extractErrorMessage(err, "Could not assign task to employee."));
    }
  };

  const handleReturnToCustomer = async () => {
    setError("");
    setSuccess("");

    try {
      await taskApi.returnToCustomer(taskId);
      const { data } = await taskApi.getById(taskId);
      setTask(data);
      setSuccess("Completed work returned to the customer.");
    } catch (err) {
      setError(extractErrorMessage(err, "Could not return task to customer."));
    }
  };

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title={task ? task.taskName : "Task details"}
      subtitle="Review the customer brief, route it to an employee, then return the finished result to the customer."
      actions={
        <div className="header-actions-cluster">
          <Link className="secondary-action" to="/manager/dashboard">
            Back to overview
          </Link>
          <button className="ghost-action danger" onClick={handleDelete}>
            Delete task
          </button>
        </div>
      }
    >
      {loading ? (
        <div className="empty-block">Loading task details...</div>
      ) : task ? (
        <section className="details-page-grid">
          <div className="panel-surface details-primary">
            <div className="panel-surface-head">
              <div>
                <h2>Task summary</h2>
                <p>{task.description}</p>
              </div>
              <span className={`task-status large ${task.status.toLowerCase()}`}>{task.status}</span>
            </div>

            <div className="details-metadata">
              <div>
                <span>Current assignee</span>
                <strong>
                  {task.assignedToByUser?.name} {task.assignedToByUser?.surname}
                </strong>
              </div>
              <div>
                <span>Customer</span>
                <strong>
                  {task.createdByUser?.name} {task.createdByUser?.surname}
                </strong>
              </div>
            </div>

            {error ? <div className="form-error">{error}</div> : null}
            {success ? <div className="form-success">{success}</div> : null}

            <div className="files-section">
              <div className="files-section-head">
                <div>
                  <h3>Forward to employee</h3>
                  <p>Choose the employee who should execute this request.</p>
                </div>
              </div>
              <div className="details-metadata">
                <div>
                  <span>Employee</span>
                  <select value={selectedEmployeeId} onChange={(event) => setSelectedEmployeeId(event.target.value)}>
                    <option value="">Select employee</option>
                    {employees.map((employee) => (
                      <option key={employee.id} value={employee.id}>
                        {employee.name} {employee.surname}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <span>Assign action</span>
                  <div className="inline-actions">
                    <button
                      className="primary-action"
                      onClick={handleAssign}
                      disabled={!selectedEmployeeId || task.status === "ReturnedToAdmin"}
                    >
                      Send to employee
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <div className="files-section">
              <div className="files-section-head">
                <div>
                  <h3>Instruction files</h3>
                  <p>Upload source materials, briefs and anything needed to start the work.</p>
                </div>
                <label className={uploading ? "primary-action disabled-action" : "primary-action"}>
                  <input type="file" onChange={handleUpload} disabled={uploading} />
                  {uploading ? "Uploading..." : "Upload file"}
                </label>
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
                  <div className="empty-inline">No attachments yet.</div>
                )}
              </div>
            </div>
          </div>

          <aside className="panel-stack">
            <section className="panel-surface">
              <div className="panel-surface-head">
                <div>
                  <h2>Execution results</h2>
                  <p>Files uploaded by the employee before the work comes back to you.</p>
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

            <section className="panel-surface">
              <div className="panel-surface-head">
                <div>
                  <h2>Manager approval</h2>
                  <p>Once the employee submits the work to you, return it to the customer from here.</p>
                </div>
              </div>
              <div className="timeline-list">
                <div>
                  <strong>Admin -> Manager</strong>
                  <span>You receive the request and prepare the brief for execution.</span>
                </div>
                <div>
                  <strong>Manager -> Employee</strong>
                  <span>Assign the right employee and attach the materials they need.</span>
                </div>
                <div>
                  <strong>Employee -> Manager -> Admin</strong>
                  <span>After submission, check the result and send it back to the customer.</span>
                </div>
              </div>
              <div className="form-actions-row">
                <button
                  className="primary-action"
                  onClick={handleReturnToCustomer}
                  disabled={task.status !== "SubmittedToManager"}
                >
                  Return to customer
                </button>
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
