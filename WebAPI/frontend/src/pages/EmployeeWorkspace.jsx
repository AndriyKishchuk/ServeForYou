import { useCallback, useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import FilePanel from "../components/FilePanel";
import TaskCard from "../components/TaskCard";
import { extractErrorMessage, fileApi, taskApi } from "../api/api";
import { useAuth } from "../context/AuthContext";

function triggerBrowserDownload(blob, fileName) {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  window.URL.revokeObjectURL(url);
}

export default function EmployeeWorkspace() {
  const { auth, logout } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [selectedTaskId, setSelectedTaskId] = useState(null);
  const [files, setFiles] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) || null,
    [tasks, selectedTaskId]
  );

  const loadTasks = useCallback(async () => {
    if (!auth?.userId) {
      return;
    }

    setIsLoading(true);
    setError("");

    try {
      const { data } = await taskApi.getAssigned(auth.userId);
      setTasks(data);

      const nextTaskId = data.some((task) => task.id === selectedTaskId)
        ? selectedTaskId
        : data[0]?.id || null;
      setSelectedTaskId(nextTaskId);

      if (nextTaskId) {
        const filesResponse = await fileApi.getByTask(nextTaskId);
        setFiles(filesResponse.data);
      } else {
        setFiles([]);
      }
    } catch (err) {
      setError(extractErrorMessage(err, "Could not load your tasks."));
    } finally {
      setIsLoading(false);
    }
  }, [auth?.userId, selectedTaskId]);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

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
          setError(extractErrorMessage(err, "Could not load task files."));
        }
      }
    }

    loadFiles();

    return () => {
      isMounted = false;
    };
  }, [selectedTaskId]);

  const handleAdvanceStatus = async (task) => {
    if (task.status === "Done") {
      return;
    }

    const nextStatus = task.status === "New" ? "InProgress" : "Done";
    setError("");
    setSuccess("");

    try {
      await taskApi.updateStatus(task.id, nextStatus);
      setSuccess(`Task moved to ${nextStatus}.`);
      await loadTasks();
    } catch (err) {
      setError(extractErrorMessage(err, "Could not update task status."));
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
      setSuccess("Result file uploaded.");
      const { data } = await fileApi.getByTask(selectedTaskId);
      setFiles(data);
    } catch (err) {
      setError(extractErrorMessage(err, "Could not upload the result file."));
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
      label: "Assigned to me",
      value: tasks.length,
      caption: "Everything in your execution queue",
    },
    {
      label: "Ready to start",
      value: tasks.filter((task) => task.status === "New").length,
      caption: "Fresh tasks waiting for action",
    },
    {
      label: "Done",
      value: tasks.filter((task) => task.status === "Done").length,
      caption: "Delivered tasks already completed",
    },
  ];

  const attachmentFiles = files.filter((file) => file.category === "Attachment");
  const resultFiles = files.filter((file) => file.category === "Result");

  return (
    <AppShell
      title="Execution workspace"
      eyebrow="Focus mode for employees"
      description="See your tasks, open manager attachments, move statuses and upload delivery files without noise."
      metrics={metrics}
      user={auth}
      onLogout={logout}
      actions={<span className="pill-label">Your tasks only</span>}
    >
      <section className="panel-card">
        <div className="panel-head">
          <div>
            <h3>My tasks</h3>
            <p>{isLoading ? "Loading your queue..." : "Open a task to see files and progress controls."}</p>
          </div>
        </div>

        {error ? <div className="form-error">{error}</div> : null}
        {success ? <div className="form-success">{success}</div> : null}

        <div className="task-list">
          {tasks.length ? (
            tasks.map((task) => (
              <TaskCard
                key={task.id}
                task={task}
                isActive={task.id === selectedTaskId}
                onSelect={(item) => setSelectedTaskId(item.id)}
                onAdvanceStatus={handleAdvanceStatus}
                subtitle={`Created by ${task.createdByUser?.name || `manager #${task.createdByUserId}`}`}
              />
            ))
          ) : (
            <div className="empty-state">
              <strong>No tasks assigned.</strong>
              <span>Your board will fill up as soon as a manager sends work your way.</span>
            </div>
          )}
        </div>
      </section>

      <section className="panel-card tall">
        <div className="panel-head">
          <div>
            <h3>Selected task</h3>
            <p>
              {selectedTask
                ? "Everything you need to execute the task is collected here."
                : "Choose a task to inspect the brief and submit results."}
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
                <span>Manager</span>
                <strong>
                  {selectedTask.createdByUser?.name} {selectedTask.createdByUser?.surname}
                </strong>
              </article>
              <article>
                <span>Next action</span>
                <strong>
                  {selectedTask.status === "New"
                    ? "Start the task"
                    : selectedTask.status === "InProgress"
                      ? "Upload result and finish"
                      : "Delivered"}
                </strong>
              </article>
            </div>

            <FilePanel
              title="Manager attachments"
              hint="Download task briefs, source files and all required materials."
              files={attachmentFiles}
              canUpload={false}
              uploadLabel=""
              category="Attachment"
              onUpload={handleUpload}
              onDownload={handleDownload}
              isUploading={false}
            />

            <FilePanel
              title="My result files"
              hint="Upload completed work, exports or any final files for the manager."
              files={resultFiles}
              canUpload={selectedTask.status !== "Done"}
              uploadLabel="Upload result"
              category="Result"
              onUpload={handleUpload}
              onDownload={handleDownload}
              isUploading={isUploading}
            />
          </div>
        ) : (
          <div className="empty-state">
            <strong>No task selected.</strong>
            <span>Select a task on the left to work with files and status updates.</span>
          </div>
        )}
      </section>
    </AppShell>
  );
}
