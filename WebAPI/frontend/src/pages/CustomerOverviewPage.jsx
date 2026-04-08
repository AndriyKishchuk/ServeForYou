import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { extractErrorMessage, taskApi, userApi } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";
import StatTile from "../components/StatTile";
import TaskListItem from "../components/TaskListItem";

export default function CustomerOverviewPage() {
  const { auth, logout } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [managers, setManagers] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    let mounted = true;

    async function loadData() {
      try {
        const [{ data: taskData }, { data: managerData }] = await Promise.all([
          taskApi.getAllCompany(),
          userApi.getManagers(),
        ]);

        if (mounted) {
          setTasks(taskData);
          setManagers(managerData);
        }
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load customer overview."));
        }
      }
    }

    loadData();

    return () => {
      mounted = false;
    };
  }, []);

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Order Flow"
      subtitle="Choose a manager, send a request, and follow how the work moves to the employee."
      actions={
        <div className="header-actions-cluster">
          <Link className="secondary-action" to="/customer/managers">
            Browse managers
          </Link>
          <Link className="primary-action" to="/customer/orders/new">
            New request
          </Link>
        </div>
      }
    >
      <section className="content-grid four-up">
        <StatTile label="Managers" value={managers.length} note="Available professionals" tone="accent" />
        <StatTile label="My requests" value={tasks.length} note="Orders created by you" tone="dark" />
        <StatTile
          label="Returned"
          value={tasks.filter((task) => task.status === "ReturnedToAdmin").length}
          note="Requests returned to you"
          tone="calm"
        />
        <StatTile
          label="Done"
          value={tasks.filter((task) => task.status === "Done").length}
          note="Requests confirmed by you"
          tone="accent"
        />
      </section>

      <section className="content-grid main-aside">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>My requests</h2>
              <p>These are the requests you placed to managers inside the system.</p>
            </div>
          </div>

          {error ? <div className="form-error">{error}</div> : null}

          <div className="task-rows">
            {tasks.length ? (
              tasks.slice(0, 8).map((task) => (
                <TaskListItem
                  key={task.id}
                  task={task}
                  to={`/customer/orders/${task.id}`}
                  extra={`${task.managerUser?.name || "Manager"} -> ${task.assignedToByUser?.name || "Employee"}`}
                />
              ))
            ) : (
              <div className="empty-block">No requests yet. Pick a manager and create the first order.</div>
            )}
          </div>
        </div>

        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>How it works</h2>
              <p>The customer flow is now centered around manager selection and work requests.</p>
            </div>
          </div>
          <div className="mini-list">
            <div className="mini-list-item">
              <strong>1. Choose a manager</strong>
              <span>Use the manager directory and compare specialization and rating.</span>
            </div>
            <div className="mini-list-item">
              <strong>2. Send the brief</strong>
              <span>Create a request with title and description directly to the chosen manager.</span>
            </div>
            <div className="mini-list-item">
              <strong>3. Manager forwards it</strong>
              <span>The manager assigns the request to an employee for execution.</span>
            </div>
            <div className="mini-list-item">
              <strong>4. Result comes back to you</strong>
              <span>The employee submits to the manager, and the manager returns the finished work to you.</span>
            </div>
          </div>
        </div>
      </section>
    </WorkspaceLayout>
  );
}
