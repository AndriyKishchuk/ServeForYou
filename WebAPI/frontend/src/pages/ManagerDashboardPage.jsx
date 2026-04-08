import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { taskApi, userApi, extractErrorMessage } from "../api/api";
import { useAuth } from "../context/AuthContext";
import WorkspaceLayout from "../components/WorkspaceLayout";
import StatTile from "../components/StatTile";
import TaskListItem from "../components/TaskListItem";

export default function ManagerDashboardPage() {
  const { auth, logout } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;

    async function loadData() {
      if (!auth?.userId) return;

      try {
        const [{ data: taskData }, { data: employeeData }] = await Promise.all([
          taskApi.getManagerInbox(),
          userApi.getCompanyEmployees(),
        ]);

        if (!mounted) return;
        setTasks(taskData);
        setEmployees(employeeData);
      } catch (err) {
        if (mounted) {
          setError(extractErrorMessage(err, "Could not load manager overview."));
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    }

    loadData();

    return () => {
      mounted = false;
    };
  }, [auth?.userId]);

  return (
    <WorkspaceLayout
      user={auth}
      onLogout={logout}
      title="Manager Inbox"
      subtitle="Customer requests land here first, then you forward them to the right employee."
      actions={<Link className="secondary-action" to="/manager/profile">Open profile</Link>}
    >
      <section className="content-grid three-up">
        <StatTile label="Requests" value={tasks.length} note="Tasks routed to you" tone="accent" />
        <StatTile
          label="Need employee"
          value={tasks.filter((task) => task.assignedToUserId === auth?.userId).length}
          note="Still waiting for forwarding"
          tone="calm"
        />
        <StatTile
          label="Ready to return"
          value={tasks.filter((task) => task.status === "SubmittedToManager").length}
          note="Waiting for manager approval"
          tone="dark"
        />
      </section>

      <section className="content-grid main-aside">
        <div className="panel-surface">
          <div className="panel-surface-head">
            <div>
              <h2>Incoming work requests</h2>
              <p>Open a request and assign it to an employee once you review the brief.</p>
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
                  to={`/manager/tasks/${task.id}`}
                  extra={`Customer: ${task.createdByUser?.name || "Unknown"} | Current assignee: ${
                    task.assignedToByUser?.name || "Not set"
                  }`}
                />
              ))
            ) : (
              <div className="empty-block">No customer requests yet.</div>
            )}
          </div>
        </div>

        <div className="panel-stack">
          <section className="panel-surface">
            <div className="panel-surface-head">
              <div>
                <h2>Manager flow</h2>
                <p>You now work between customer requests and employee execution.</p>
              </div>
            </div>
            <div className="action-list">
              <Link className="action-card" to="/manager/profile">
                <strong>View profile</strong>
                <span>Check how customers see your manager card and specialization.</span>
              </Link>
              {tasks[0] ? (
                <Link className="action-card" to={`/manager/tasks/${tasks[0].id}`}>
                  <strong>Open latest request</strong>
                  <span>Review the brief, forward it to an employee, then return the result.</span>
                </Link>
              ) : null}
            </div>
          </section>

          <section className="panel-surface">
            <div className="panel-surface-head">
              <div>
                <h2>Company employees</h2>
                <p>These are the employees you can use for the execution stage.</p>
              </div>
            </div>
            <div className="mini-list">
              {employees.length ? (
                employees.map((employee) => (
                  <div key={employee.id} className="mini-list-item">
                    <strong>
                      {employee.name} {employee.surname}
                    </strong>
                    <span>{employee.email}</span>
                  </div>
                ))
              ) : (
                <div className="empty-inline">No employees found in your company.</div>
              )}
            </div>
          </section>
        </div>
      </section>
    </WorkspaceLayout>
  );
}
