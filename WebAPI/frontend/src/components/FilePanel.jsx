function formatFileSize(bytes) {
  if (!bytes && bytes !== 0) return "-";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function formatDate(value) {
  return new Intl.DateTimeFormat("uk-UA", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

export default function FilePanel({
  title,
  hint,
  files,
  canUpload,
  uploadLabel,
  category,
  onUpload,
  onDownload,
  isUploading,
}) {
  return (
    <section className="panel-card">
      <div className="panel-head">
        <div>
          <h3>{title}</h3>
          <p>{hint}</p>
        </div>
        {canUpload ? (
          <label className={isUploading ? "upload-button disabled" : "upload-button"}>
            <input
              type="file"
              disabled={isUploading}
              onChange={(event) => {
                const file = event.target.files?.[0];
                if (file) {
                  onUpload(file, category);
                }
                event.target.value = "";
              }}
            />
            {isUploading ? "Uploading..." : uploadLabel}
          </label>
        ) : null}
      </div>

      <div className="file-list">
        {files.length ? (
          files.map((file) => (
            <article key={file.id} className="file-item">
              <div>
                <strong>{file.fileName}</strong>
                <span>
                  {file.uploadedByName} • {formatDate(file.uploadedAt)} • {formatFileSize(file.fileSize)}
                </span>
              </div>
              <button className="ghost-button" onClick={() => onDownload(file)}>
                Download
              </button>
            </article>
          ))
        ) : (
          <div className="empty-state compact">{hint}</div>
        )}
      </div>
    </section>
  );
}
