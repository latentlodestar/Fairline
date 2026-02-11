import { useGetStatusQuery } from "../api/api";

export function StatusFooter() {
  const { data: status } = useGetStatusQuery();

  return (
    <footer className="status-footer">
      <span>API: <span className={status ? "status-ok" : "status-err"}>{status ? "Healthy" : "Down"}</span></span>
      <span>Database: <span className={status?.databaseConnected ? "status-ok" : "status-err"}>{status?.databaseConnected ? "Connected" : "Disconnected"}</span></span>
      <span className="status-footer__version">v{status?.version ?? "—"}</span>
    </footer>
  );
}
