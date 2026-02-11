import { useGetStatusQuery, useGetRunsQuery, useGetDashboardQuery } from "../api/api";
import { KpiCard } from "../components/KpiCard";
import { Card, CardHeader, CardBody } from "../components/Card";
import { Badge } from "../components/Badge";
import { Table, Th, Td, Tr } from "../components/Table";

export function StatusPage() {
  const { data: status, isLoading: statusLoading, error: statusError } = useGetStatusQuery();
  const { data: runs, isLoading: runsLoading } = useGetRunsQuery(5);
  const { data: dashboard } = useGetDashboardQuery();

  if (statusLoading || runsLoading) {
    return (
      <div className="page">
        <p>Loading...</p>
      </div>
    );
  }

  if (statusError) {
    return (
      <div className="page">
        <Card status="error">
          <CardHeader>Error</CardHeader>
          <CardBody>
            <p>{"status" in statusError ? `HTTP ${statusError.status}` : statusError.message}</p>
          </CardBody>
        </Card>
      </div>
    );
  }

  const lastCapture = dashboard?.kpis.latestCaptureUtc
    ? new Date(dashboard.kpis.latestCaptureUtc).toLocaleString()
    : "Never";

  return (
    <div className="page">
      <h1 className="section-title">System Status</h1>

      <div className="kpi-strip">
        <KpiCard label="API" value={status ? "Healthy" : "Down"} />
        <KpiCard label="Database" value={status?.databaseConnected ? "Connected" : "Disconnected"} />
        <KpiCard label="Version" value={status?.version ?? "—"} />
        <KpiCard label="Last Capture" value={lastCapture} />
      </div>

      <Card>
        <CardHeader>Latest Ingestion Runs</CardHeader>
        {(!runs || runs.length === 0) ? (
          <CardBody>
            <p className="placeholder">No ingestion runs yet.</p>
          </CardBody>
        ) : (
          <Table>
            <thead>
              <tr>
                <Th>Type</Th>
                <Th>Status</Th>
                <Th>Started</Th>
                <Th>Completed</Th>
                <Th align="right">Events</Th>
                <Th align="right">Snapshots</Th>
                <Th align="right">Errors</Th>
              </tr>
            </thead>
            <tbody>
              {runs.map((run) => (
                <Tr key={run.id}>
                  <Td>
                    <Badge variant="neutral">{run.runType}</Badge>
                  </Td>
                  <Td>
                    <Badge
                      variant={
                        run.status === "Completed"
                          ? "success"
                          : run.status === "Failed"
                            ? "danger"
                            : "warning"
                      }
                    >
                      {run.status}
                    </Badge>
                  </Td>
                  <Td>{new Date(run.startedAtUtc).toLocaleString()}</Td>
                  <Td>
                    {run.completedAtUtc
                      ? new Date(run.completedAtUtc).toLocaleString()
                      : "—"}
                  </Td>
                  <Td align="right">{run.eventCount}</Td>
                  <Td align="right">{run.snapshotCount}</Td>
                  <Td align="right">{run.errorCount}</Td>
                </Tr>
              ))}
            </tbody>
          </Table>
        )}
      </Card>
    </div>
  );
}
