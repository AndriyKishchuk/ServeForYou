import { useEffect, useState } from "react";
import { taskApi, extractErrorMessage } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";
import StatTile from "../components/StatTile";
import TaskListItem from "../components/TaskListItem";

export default function EmployeeTasksPage() {
  const { auth, logout } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;

    async function loadTasks() {
      if (!auth?.userId) return;

      try {
        const { data } = await taskApi.getAssigned(auth.userId);
        if (mounted) {
          setTasks(data);
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load your tasks."));
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    }

    loadTasks();

    return () => {
      mounted = false;
    };
  }, [auth?.userId]);

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="My Tasks"
      subtitle="A focused queue with every assigned task on its own details page."
      actions={<div className="workspace-badge">Employee workspace</div>}
    >
      <section className="content-grid three-up">
        <StatTile label="Assigned" value={tasks.length} note="All tasks sent to you" tone="accent" />
        <StatTile
          label="To start"
          value={tasks.filter((task) => task.status === "New").length}
          note="Tasks waiting for action"
          tone="dark"
        />
        <StatTile
          label="Sent back"
          value={tasks.filter((task) => task.status === "SubmittedToManager").length}
          note="Completed and returned to manager"
          tone="calm"
        />
      </section>

      <section className="panel-surface">
        <div className="panel-surface-head">
          <div>
            <h2>Assigned queue</h2>
            <p>Open any task to download instructions, update status and upload result files.</p>
          </div>
        </div>

        {error ? <div className="form-error">{error}</div> : null}

        <div className="task-rows">
          {loading ? (
            <div className="empty-block">Loading tasks...</div>
          ) : tasks.length ? (
            tasks.map((task) => (
              <TaskListItem
                key={task.id}
                task={task}
                to={`/employee/tasks/${task.id}`}
                extra={`Manager: ${`${task.managerUser?.name || ""} ${task.managerUser?.surname || ""}`.trim() || "Not set"}`}
              />
            ))
          ) : (
            <div className="empty-block">No tasks assigned right now.</div>
          )}
        </div>
      </section>
    </WorkspaceLayout>
  );
}
