export default function StatTile({ label, value, note, tone = "default" }) {
  return (
    <article className={`stat-tile ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{note}</small>
    </article>
  );
}
