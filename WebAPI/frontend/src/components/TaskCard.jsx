function formatDate(value) {
  if (!value) return "Unknown";

  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function statusClass(status) {
  switch (status) {
    case "Done":
      return "done";
    case "InProgress":
      return "progress";
    default:
      return "new";
  }
}

export default function TaskCard({
  task,
  subtitle,
  onSelect,
  onDelete,
  onAdvanceStatus,
  isActive,
}) {
  return (
    <article
      className={isActive ? "task-card active" : "task-card"}
      onClick={() => onSelect?.(task)}
      role="button"
      tabIndex={0}
      onKeyDown={(event) => {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          onSelect?.(task);
        }
      }}
    >
      <div className="task-card-top">
        <span className={`status-pill ${statusClass(task.status)}`}>{task.status}</span>
        <span className="task-date">{formatDate(task.createdAt)}</span>
      </div>

      <h3>{task.taskName}</h3>
      <p>{task.description}</p>
      <strong>{subtitle}</strong>

      <div className="task-card-actions">
        {onAdvanceStatus ? (
          <button
            className="secondary-button"
            onClick={(event) => {
              event.stopPropagation();
              onAdvanceStatus(task);
            }}
          >
            {task.status === "New" ? "Start work" : task.status === "InProgress" ? "Mark done" : "Completed"}
          </button>
        ) : null}

        {onDelete ? (
          <button
            className="ghost-button danger"
            onClick={(event) => {
              event.stopPropagation();
              onDelete(task.id);
            }}
          >
            Delete
          </button>
        ) : null}
      </div>
    </article>
  );
}
