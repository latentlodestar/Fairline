import { useGetStatusQuery, useGetEdgeComparisonsQuery } from "../api/api";
import { formatDateTime } from "../lib/format";

export function StatusFooter() {
  const { data: status } = useGetStatusQuery();
  const { data: edges } = useGetEdgeComparisonsQuery();
  const lastCapture = edges?.kpis.latestCaptureUtc;

  return (
    <footer className="status-footer">
      <span>API: <span className={status ? "status-ok" : "status-err"}>{status ? "Healthy" : "Down"}</span></span>
      <span>Database: <span className={status?.databaseConnected ? "status-ok" : "status-err"}>{status?.databaseConnected ? "Connected" : "Disconnected"}</span></span>
      {lastCapture && <span>Last Capture: {formatDateTime(lastCapture)}</span>}
      <span className="status-footer__version">v{status?.version ?? "—"}</span>
    </footer>
  );
}
