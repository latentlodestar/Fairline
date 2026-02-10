import { useEffect, useState } from "react";
import { api } from "../api/client";
import { StatusCard } from "../components/StatusCard";
import type { ApiStatusResponse, ProviderInfo, ScenarioSummary } from "../types";
import "./StatusPage.css";

export function StatusPage() {
  const [status, setStatus] = useState<ApiStatusResponse | null>(null);
  const [providers, setProviders] = useState<ProviderInfo[]>([]);
  const [scenarios, setScenarios] = useState<ScenarioSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const [statusRes, providersRes, scenariosRes] = await Promise.all([
          api.getStatus(),
          api.getProviders(),
          api.getScenarios(),
        ]);
        setStatus(statusRes);
        setProviders(providersRes);
        setScenarios(scenariosRes);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load data");
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  if (loading) {
    return <div className="status-page"><p>Loading...</p></div>;
  }

  if (error) {
    return (
      <div className="status-page">
        <StatusCard title="Error" status="error">
          <p>{error}</p>
        </StatusCard>
      </div>
    );
  }

  return (
    <div className="status-page">
      <h2>Dashboard</h2>

      <div className="status-grid">
        <StatusCard
          title="API Health"
          status={status ? "ok" : "error"}
        >
          <p>Version: {status?.version}</p>
          <p>Timestamp: {status?.timestamp ? new Date(status.timestamp).toLocaleString() : "N/A"}</p>
        </StatusCard>

        <StatusCard
          title="Database"
          status={status?.databaseConnected ? "ok" : "error"}
        >
          <p>{status?.databaseConnected ? "Connected" : "Disconnected"}</p>
        </StatusCard>

        <StatusCard title="Latest Odds Pulls">
          {providers.length === 0 ? (
            <p className="placeholder">No providers configured yet</p>
          ) : (
            <ul>
              {providers.map((p) => (
                <li key={p.id}>{p.name} ({p.slug})</li>
              ))}
            </ul>
          )}
        </StatusCard>

        <StatusCard title="Scenario Comparisons">
          {scenarios.length === 0 ? (
            <p className="placeholder">No scenarios created yet</p>
          ) : (
            <ul>
              {scenarios.map((s) => (
                <li key={s.id}>{s.name} ({s.comparisonCount} comparisons)</li>
              ))}
            </ul>
          )}
        </StatusCard>
      </div>
    </div>
  );
}
