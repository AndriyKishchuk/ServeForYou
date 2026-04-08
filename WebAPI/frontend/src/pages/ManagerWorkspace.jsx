import { useCallback, useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import FilePanel from "../components/FilePanel";
import TaskCard from "../components/TaskCard";
import { extractErrorMessage, fileApi, taskApi, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";

const emptyTaskForm = {
  taskName: "",
  taskDescription: "",
  assignedToUserId: "",
};

function triggerBrowserDownload(blob, fileName) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  window.URL.revokeObjectURL(url);
}

export default function ManagerWorkspace() {
  const { auth, logout } = useAuth();
  const [employees, setEmployees] = useState([]);
  const [tasks, setTasks] = useState([]);
  const [selectedTaskId, setSelectedTaskId] = useState(null);
  const [files, setFiles] = useState([]);
  const [taskForm, setTaskForm] = useState(emptyTaskForm);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [isUploading, setIsUploading] = useState(false);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) || null,
    [tasks, selectedTaskId]
  );

  const loadDashboard = useCallback(async () => {
    if (!auth?.userId) {
      return;
    }

    setIsLoading(true);
    setError("");

    try {
      const [{ data: employeeData }, { data: taskData }] = await Promise.all([
        userApi.getCompanyEmployees(),
        taskApi.getCreated(auth.userId),
      ]);

      setEmployees(employeeData);
      setTasks(taskData);

      const nextTaskId = taskData.some((task) => task.id === selectedTaskId)
        ? selectedTaskId
        : taskData[0]?.id || null;
      setSelectedTaskId(nextTaskId);

      if (nextTaskId) {
        const { data: fileData } = await fileApi.getByTask(nextTaskId);
        setFiles(fileData);
      } else {
        setFiles([]);
      }
    } catch (err) {
      setError(extractErrorMessage(err, "Could not load manager workspace."));
    } finally {
      setIsLoading(false);
    }
  }, [auth?.userId, selectedTaskId]);

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  useEffect(() => {
    let isMounted = true;

    async function loadFiles() {
      if (!selectedTaskId) {
        setFiles([]);
        return;
      }

      try {
        const { data } = await fileApi.getByTask(selectedTaskId);
        if (isMounted) {
          setFiles(data);
        }
      } catch (err) {
        if (isMounted) {
          setError(extractErrorMessage(err, "Could not load files for this task."));
        }
      }
    }

    loadFiles();

    return () => {
      isMounted = false;
    };
  }, [selectedTaskId]);

  const handleCreateTask = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");
    setIsCreating(true);

    try {
      await taskApi.create({
        taskName: taskForm.taskName,
        taskDescription: taskForm.taskDescription,
        assignedToUserId: Number(taskForm.assignedToUserId),
      });

      setTaskForm(emptyTaskForm);
      setSuccess("Task created and assigned successfully.");
      await loadDashboard();
    } catch (err) {
      setError(extractErrorMessage(err, "Could not create the task."));
    } finally {
      setIsCreating(false);
    }
  };

  const handleDeleteTask = async (taskId) => {
    setError("");
    setSuccess("");

    try {
      await taskApi.delete(taskId);
      setSuccess("Task removed.");
      if (selectedTaskId === taskId) {
        setSelectedTaskId(null);
      }
      await loadDashboard();
    } catch (err) {
      setError(extractErrorMessage(err, "Could not delete the task."));
    }
  };

  const handleUpload = async (file, category) => {
    if (!selectedTaskId) {
      return;
    }

    setError("");
    setSuccess("");
    setIsUploading(true);

    try {
      await fileApi.upload(selectedTaskId, file, category);
      setSuccess("Attachment uploaded.");
      const { data } = await fileApi.getByTask(selectedTaskId);
      setFiles(data);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not upload the file."));
    } finally {
      setIsUploading(false);
    }
  };

  const handleDownload = async (file) => {
    setError("");

    try {
      const response = await fileApi.download(file.id);
      triggerBrowserDownload(response.data, file.fileName);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not download the file."));
    }
  };

  const metrics = [
    {
      label: "Created tasks",
      value: tasks.length,
      caption: "All tasks authored by you",
    },
    {
      label: "In progress",
      value: tasks.filter((task) => task.status === "InProgress").length,
      caption: "Currently being executed",
    },
    {
      label: "Employees",
      value: employees.length,
      caption: "Available in your company",
    },
  ];

  const attachmentFiles = files.filter((file) => file.category === "Attachment");
  const resultFiles = files.filter((file) => file.category === "Result");

  return (
    <AppShell
      title="Manager command center"
      eyebrow="Assign work with confidence"
      description="Spin up tasks, keep attachments close, and follow delivery from one premium workspace."
      metrics={metrics}
      user={auth}
      onLogout={logout}
      actions={<span className="pill-label">Role-based redirect active</span>}
    >
      <section className="panel-card tall">
        <div className="panel-head">
          <div>
            <h3>Create and assign a task</h3>
            <p>Only employees from your company are available for assignment.</p>
          </div>
        </div>

        <form className="stack-form" onSubmit={handleCreateTask}>
          <label>
            Task title
            <input
              value={taskForm.taskName}
              onChange={(event) => setTaskForm((current) => ({ ...current, taskName: event.target.value }))}
              placeholder="Prepare monthly analytics summary"
              required
            />
          </label>

          <label>
            Description
            <textarea
              value={taskForm.taskDescription}
              onChange={(event) =>
                setTaskForm((current) => ({ ...current, taskDescription: event.target.value }))
              }
              placeholder="Describe the outcome, deadlines and source materials."
              rows={5}
              required
            />
          </label>

          <label>
            Assign to employee
            <select
              value={taskForm.assignedToUserId}
              onChange={(event) =>
                setTaskForm((current) => ({ ...current, assignedToUserId: event.target.value }))
              }
              required
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

          <button className="primary-button" type="submit" disabled={isCreating || !employees.length}>
            {isCreating ? "Creating..." : "Create task"}
          </button>
        </form>
      </section>

      <section className="panel-card">
        <div className="panel-head">
          <div>
            <h3>Created tasks</h3>
            <p>{isLoading ? "Loading task flow..." : "Select a task to inspect files and execution state."}</p>
          </div>
        </div>

        <div className="task-list">
          {tasks.length ? (
            tasks.map((task) => (
              <TaskCard
                key={task.id}
                task={task}
                isActive={task.id === selectedTaskId}
                onSelect={(item) => setSelectedTaskId(item.id)}
                onDelete={handleDeleteTask}
                subtitle={`Assigned to ${task.assignedToByUser?.name || `employee #${task.assignedToUserId}`}`}
              />
            ))
          ) : (
            <div className="empty-state">
              <strong>No tasks yet.</strong>
              <span>Create the first task and your manager board will come alive.</span>
            </div>
          )}
        </div>
      </section>

      <section className="panel-card tall">
        <div className="panel-head">
          <div>
            <h3>Task detail studio</h3>
            <p>
              {selectedTask
                ? "Review context, share source files and watch for execution results."
                : "Pick a task from the list to see details."}
            </p>
          </div>
        </div>

        {selectedTask ? (
          <div className="detail-stack">
            <div className="detail-hero">
              <span className="pill-label">{selectedTask.status}</span>
              <h3>{selectedTask.taskName}</h3>
              <p>{selectedTask.description}</p>
            </div>

            <div className="detail-meta">
              <article>
                <span>Assigned employee</span>
                <strong>
                  {selectedTask.assignedToByUser?.name} {selectedTask.assignedToByUser?.surname}
                </strong>
              </article>
              <article>
                <span>Created by</span>
                <strong>You</strong>
              </article>
            </div>

            <FilePanel
              title="Instruction attachments"
              hint="Upload briefs, PDFs, docs or assets for the employee."
              files={attachmentFiles}
              canUpload
              uploadLabel="Upload attachment"
              category="Attachment"
              onUpload={handleUpload}
              onDownload={handleDownload}
              isUploading={isUploading}
            />

            <FilePanel
              title="Execution results"
              hint="Employee-uploaded results will appear here."
              files={resultFiles}
              canUpload={false}
              uploadLabel=""
              category="Result"
              onUpload={handleUpload}
              onDownload={handleDownload}
              isUploading={false}
            />
          </div>
        ) : (
          <div className="empty-state">
            <strong>No task selected.</strong>
            <span>Choose a task to manage its files and results.</span>
          </div>
        )}
      </section>
    </AppShell>
  );
}
