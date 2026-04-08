import { useState, useEffect } from "react";
import { taskApi } from "../api/api";

export default function EmployeeDashboard() {
  const user = JSON.parse(localStorage.getItem("user"));
  const [tasks, setTasks] = useState([]);

  const loadTasks = () => {
    taskApi.getAssigned(user.userId).then((res) => setTasks(res.data));
  };

  useEffect(loadTasks, [user.userId]);

  const handleStatus = async (id, newStatus) => {
    await taskApi.updateStatus(id, newStatus);
    loadTasks();
  };

  return (
    <div style={{ maxWidth: 600, margin: "40px auto" }}>
      <h2>Employee Dashboard — {user.name}</h2>

      {tasks.map((t) => (
        <div key={t.id} style={{ border: "1px solid #ccc", padding: 10, marginBottom: 10 }}>
          <strong>{t.taskName}</strong> — {t.status}
          <p>{t.description}</p>
          <p>Created by: {t.createdByUser?.name ?? t.createdByUserId}</p>
          <div>
            {t.status === "New" && (
              <button onClick={() => handleStatus(t.id, "InProgress")}>Start</button>
            )}
            {t.status === "InProgress" && (
              <button onClick={() => handleStatus(t.id, "Done")}>Done</button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}