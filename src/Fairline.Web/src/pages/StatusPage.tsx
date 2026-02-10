import {
  useGetStatusQuery,
  useGetProvidersQuery,
  useGetScenariosQuery,
} from "../api/api";
import { StatusCard } from "../components/StatusCard";

export function StatusPage() {
  const { data: status, isLoading: statusLoading, error: statusError } = useGetStatusQuery();
  const { data: providers, isLoading: providersLoading } = useGetProvidersQuery();
  const { data: scenarios, isLoading: scenariosLoading } = useGetScenariosQuery();

  const loading = statusLoading || providersLoading || scenariosLoading;

  if (loading) {
    return (
      <div className="page">
        <p>Loading...</p>
      </div>
    );
  }

  if (statusError) {
    return (
      <div className="page">
        <StatusCard title="Error" status="error">
          <p>{"status" in statusError ? `HTTP ${statusError.status}` : statusError.message}</p>
        </StatusCard>
      </div>
    );
  }

  return (
    <div className="page">
      <h2 className="section-title">System Status</h2>

      <div className="card-grid">
        <StatusCard title="API Health" status={status ? "ok" : "error"}>
          <p>Version: {status?.version}</p>
          <p>
            Timestamp:{" "}
            {status?.timestamp
              ? new Date(status.timestamp).toLocaleString()
              : "N/A"}
          </p>
        </StatusCard>

        <StatusCard
          title="Database"
          status={status?.databaseConnected ? "ok" : "error"}
        >
          <p>{status?.databaseConnected ? "Connected" : "Disconnected"}</p>
        </StatusCard>

        <StatusCard title="Latest Odds Pulls">
          {!providers || providers.length === 0 ? (
            <p className="placeholder">No providers configured yet</p>
          ) : (
            <ul>
              {providers.map((p) => (
                <li key={p.id}>
                  {p.name} ({p.slug})
                </li>
              ))}
            </ul>
          )}
        </StatusCard>

        <StatusCard title="Scenario Comparisons">
          {!scenarios || scenarios.length === 0 ? (
            <p className="placeholder">No scenarios created yet</p>
          ) : (
            <ul>
              {scenarios.map((s) => (
                <li key={s.id}>
                  {s.name} ({s.comparisonCount} comparisons)
                </li>
              ))}
            </ul>
          )}
        </StatusCard>
      </div>
    </div>
  );
}
