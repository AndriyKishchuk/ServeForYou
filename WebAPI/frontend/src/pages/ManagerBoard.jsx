import { useState, useEffect } from "react";
import { taskApi } from "../api/api";

export default function ManagerDashboard() {
  const user = JSON.parse(localStorage.getItem("user"));
  const [tasks, setTasks] = useState([]);
  const [form, setForm] = useState({ taskName: "", taskDescription: "", assignedToUserId: "" });

  const loadTasks = () => {
    taskApi.getCreated(user.userId).then((res) => setTasks(res.data));
  };

  useEffect(loadTasks, [user.userId]);

  const handleCreate = async (e) => {
    e.preventDefault();
    await taskApi.create({
      ...form,
      createdByUserId: user.userId,
      assignedToUserId: parseInt(form.assignedToUserId),
    });
    setForm({ taskName: "", taskDescription: "", assignedToUserId: "" });
    loadTasks();
  };

  const handleDelete = async (id) => {
    await taskApi.delete(id);
    loadTasks();
  };

  return (
    <div style={{ maxWidth: 600, margin: "40px auto" }}>
      <h2>Manager Dashboard — {user.name}</h2>

      <h3>Create Task</h3>
      <form onSubmit={handleCreate}>
        <input placeholder="Task name" value={form.taskName} onChange={(e) => setForm({ ...form, taskName: e.target.value })} required style={{ display: "block", width: "100%", marginBottom: 10, padding: 8 }} />
        <input placeholder="Description" value={form.taskDescription} onChange={(e) => setForm({ ...form, taskDescription: e.target.value })} required style={{ display: "block", width: "100%", marginBottom: 10, padding: 8 }} />
        <input placeholder="Assigned to (User ID)" value={form.assignedToUserId} onChange={(e) => setForm({ ...form, assignedToUserId: e.target.value })} required style={{ display: "block", width: "100%", marginBottom: 10, padding: 8 }} />
        <button type="submit" style={{ padding: 10 }}>Create</button>
      </form>

      <h3>My Tasks</h3>
      {tasks.map((t) => (
        <div key={t.id} style={{ border: "1px solid #ccc", padding: 10, marginBottom: 10 }}>
          <strong>{t.taskName}</strong> — {t.status}
          <p>{t.description}</p>
          <p>Assigned to: {t.assignedToByUser?.name ?? t.assignedToUserId}</p>
          <button onClick={() => handleDelete(t.id)} style={{ color: "red" }}>Delete</button>
        </div>
      ))}
    </div>
  );
}