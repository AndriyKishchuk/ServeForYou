import { Link } from "react-router-dom";
import { getTaskStatusLabel } from "../utils/taskStatus";

function formatDate(value) {
  if (!value) return "No date";

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  }).format(new Date(value));
}

export default function TaskListItem({ task, to, extra, badgeTone = "neutral" }) {
  return (
    <Link className="task-row" to={to}>
      <div className="task-row-main">
        <div className="task-row-head">
          <strong>{task.taskName}</strong>
          <span className={`task-status ${badgeTone} ${task.status.toLowerCase()}`}>
            {getTaskStatusLabel(task.status)}
          </span>
        </div>
        <p>{task.description}</p>
      </div>
      <div className="task-row-meta">
        <span>{extra}</span>
        <small>{formatDate(task.createdAt)}</small>
      </div>
    </Link>
  );
}
