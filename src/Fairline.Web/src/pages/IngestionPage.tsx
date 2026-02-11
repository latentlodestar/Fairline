import { useCallback, useEffect, useRef, useState } from "react";
import { Button } from "../components/Button";
import { Card, CardBody, CardHeader } from "../components/Card";
import { Badge } from "../components/Badge";
import { Table, Th, Td, Tr } from "../components/Table";
import { cn } from "../lib/cn";
import {
  api,
  useGetCatalogQuery,
  useRefreshCatalogMutation,
  useToggleTrackedLeagueMutation,
  useRunIngestionMutation,
  useGetRunsQuery,
} from "../api/api";
import { useAppDispatch } from "../store";
import type {
  SseLogEvent,
  SseProgressEvent,
  SseSummaryEvent,
} from "../types";

export function IngestionPage() {
  return (
    <div className="page">
      <h1 className="section-title">Ingestion</h1>
      <CatalogSection />
      <RunIngestionSection />
      <RecentRunsSection />
    </div>
  );
}

/* ----------------------------------------------------------------
   CATALOG SECTION
   ---------------------------------------------------------------- */

function CatalogSection() {
  const { data: catalog, isLoading } = useGetCatalogQuery();
  const [refreshCatalog, { isLoading: isRefreshing }] =
    useRefreshCatalogMutation();
  const [toggleLeague] = useToggleTrackedLeagueMutation();
  const [inSeasonOnly, setInSeasonOnly] = useState(false);

  const trackedMap = new Map(
    catalog?.trackedLeagues?.map((t) => [t.providerSportKey, t.enabled]) ?? [],
  );

  const sports = inSeasonOnly
    ? (catalog?.sports ?? []).filter((s) => s.active)
    : (catalog?.sports ?? []);

  const grouped = new Map<string, typeof catalog.sports>();
  for (const sport of sports) {
    const group = sport.group;
    if (!grouped.has(group)) grouped.set(group, []);
    grouped.get(group)!.push(sport);
  }

  return (
    <Card>
      <CardHeader>
        <span>Sports Catalog</span>
        <div className="ingest-catalog-actions">
          <label className="ingest-toggle">
            <input
              type="checkbox"
              checked={inSeasonOnly}
              onChange={() => setInSeasonOnly((v) => !v)}
            />
            <span className="ingest-toggle__label">In Season Only</span>
          </label>
          <Button
            size="sm"
            variant="secondary"
            disabled={isRefreshing}
            onClick={() => refreshCatalog()}
          >
            {isRefreshing ? "Refreshing..." : "Refresh Catalog"}
          </Button>
        </div>
      </CardHeader>
      <CardBody>
        {isLoading && <p className="ingest-muted">Loading catalog...</p>}
        {!isLoading && grouped.size === 0 && (
          <p className="ingest-muted">
            No sports in catalog. Click &quot;Refresh Catalog&quot; to fetch
            from the Odds API.
          </p>
        )}
        {[...grouped.entries()].map(([group, sports]) => (
          <div key={group} className="ingest-catalog-group">
            <div className="ingest-catalog-group__title">{group}</div>
            <div className="ingest-catalog-group__items">
              {sports.map((sport) => {
                const tracked = trackedMap.get(sport.providerSportKey) ?? false;
                return (
                  <label key={sport.providerSportKey} className="ingest-toggle">
                    <input
                      type="checkbox"
                      checked={tracked}
                      onChange={() =>
                        toggleLeague({
                          providerSportKey: sport.providerSportKey,
                          enabled: !tracked,
                        })
                      }
                    />
                    <span className="ingest-toggle__label">
                      {sport.title}
                      {!sport.active && (
                        <Badge variant="neutral">Off-season</Badge>
                      )}
                    </span>
                  </label>
                );
              })}
            </div>
          </div>
        ))}
      </CardBody>
    </Card>
  );
}

/* ----------------------------------------------------------------
   INGESTION PRESETS (localStorage)
   ---------------------------------------------------------------- */

interface IngestionPreset {
  name: string;
  windowHours: number;
  regions: string;
  markets: string;
  books: string;
}

const PRESETS_KEY = "fairline:ingestion-presets";
const ACTIVE_PRESET_KEY = "fairline:ingestion-active-preset";

const DEFAULT_VALUES: Omit<IngestionPreset, "name"> = {
  windowHours: 72,
  regions: "us",
  markets: "h2h,spreads,totals",
  books: "draftkings,pinnacle",
};

function loadPresets(): IngestionPreset[] {
  try {
    return JSON.parse(localStorage.getItem(PRESETS_KEY) ?? "[]");
  } catch {
    return [];
  }
}

function savePresets(presets: IngestionPreset[]) {
  localStorage.setItem(PRESETS_KEY, JSON.stringify(presets));
}

function loadActivePresetName(): string | null {
  return localStorage.getItem(ACTIVE_PRESET_KEY);
}

function saveActivePresetName(name: string | null) {
  if (name) {
    localStorage.setItem(ACTIVE_PRESET_KEY, name);
  } else {
    localStorage.removeItem(ACTIVE_PRESET_KEY);
  }
}

/* ----------------------------------------------------------------
   RUN INGESTION SECTION
   ---------------------------------------------------------------- */

function RunIngestionSection() {
  const [presets, setPresets] = useState(loadPresets);
  const [activePresetName, setActivePresetName] = useState(loadActivePresetName);

  const initial = presets.find((p) => p.name === activePresetName) ?? DEFAULT_VALUES;
  const [regions, setRegions] = useState(initial.regions);
  const [markets, setMarkets] = useState(initial.markets);
  const [books, setBooks] = useState(initial.books);
  const [windowHours, setWindowHours] = useState(initial.windowHours);

  const [runIngestion] = useRunIngestionMutation();
  const dispatch = useAppDispatch();

  const [activeRunId, setActiveRunId] = useState<string | null>(null);
  const [logs, setLogs] = useState<SseLogEvent[]>([]);
  const [progress, setProgress] = useState<SseProgressEvent | null>(null);
  const [summary, setSummary] = useState<SseSummaryEvent | null>(null);
  const [isRunning, setIsRunning] = useState(false);
  const logEndRef = useRef<HTMLDivElement>(null);

  const applyPreset = (name: string | null) => {
    const preset = presets.find((p) => p.name === name);
    const values = preset ?? DEFAULT_VALUES;
    setWindowHours(values.windowHours);
    setRegions(values.regions);
    setMarkets(values.markets);
    setBooks(values.books);
    setActivePresetName(name);
    saveActivePresetName(name);
  };

  const handleSavePreset = () => {
    const name = window.prompt("Preset name:", activePresetName ?? "");
    if (!name?.trim()) return;
    const trimmed = name.trim();
    const preset: IngestionPreset = { name: trimmed, windowHours, regions, markets, books };
    const next = presets.filter((p) => p.name !== trimmed).concat(preset);
    setPresets(next);
    savePresets(next);
    setActivePresetName(trimmed);
    saveActivePresetName(trimmed);
  };

  const handleDeletePreset = () => {
    if (!activePresetName) return;
    const next = presets.filter((p) => p.name !== activePresetName);
    setPresets(next);
    savePresets(next);
    setActivePresetName(null);
    saveActivePresetName(null);
  };

  const scrollToBottom = useCallback(() => {
    logEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [logs, scrollToBottom]);

  useEffect(() => {
    if (!activeRunId) return;

    const es = new EventSource(`/api/ingest/runs/${activeRunId}/stream`);

    es.addEventListener("log", (e) => {
      const data: SseLogEvent = JSON.parse(e.data);
      setLogs((prev) => [...prev, data]);
    });

    es.addEventListener("progress", (e) => {
      const data: SseProgressEvent = JSON.parse(e.data);
      setProgress(data);
    });

    es.addEventListener("summary", (e) => {
      const data: SseSummaryEvent = JSON.parse(e.data);
      setSummary(data);
      setIsRunning(false);
      dispatch(api.util.invalidateTags(["Runs"]));
      es.close();
    });

    es.onerror = () => {
      setIsRunning(false);
      dispatch(api.util.invalidateTags(["Runs"]));
      es.close();
    };

    return () => es.close();
  }, [activeRunId, dispatch]);

  const handleRun = async () => {
    setLogs([]);
    setProgress(null);
    setSummary(null);
    setIsRunning(true);

    const result = await runIngestion({
      windowHours,
      regions: regions
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
      markets: markets
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
      books: books
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
    }).unwrap();

    setActiveRunId(result.runId);
  };

  return (
    <>
      <Card>
        <CardHeader>
          <span>Run Gap-Fill Ingestion</span>
          <div className="ingest-preset-bar">
            <select
              className="input ingest-preset-bar__select"
              value={activePresetName ?? ""}
              onChange={(e) => applyPreset(e.target.value || null)}
            >
              <option value="">Defaults</option>
              {presets.map((p) => (
                <option key={p.name} value={p.name}>
                  {p.name}
                </option>
              ))}
            </select>
            <Button size="sm" variant="secondary" onClick={handleSavePreset}>
              Save
            </Button>
            {activePresetName && (
              <Button size="sm" variant="secondary" onClick={handleDeletePreset}>
                Delete
              </Button>
            )}
          </div>
        </CardHeader>
        <CardBody>
          <div className="ingest-form">
            <div className="ingest-form__row">
              <label className="ingest-form__label">
                Window (hours)
                <input
                  type="number"
                  className="input"
                  value={windowHours}
                  onChange={(e) => setWindowHours(Number(e.target.value))}
                  min={1}
                  max={720}
                />
              </label>
              <label className="ingest-form__label">
                Regions
                <input
                  className="input"
                  value={regions}
                  onChange={(e) => setRegions(e.target.value)}
                  placeholder="us,eu"
                />
              </label>
              <label className="ingest-form__label">
                Markets
                <input
                  className="input"
                  value={markets}
                  onChange={(e) => setMarkets(e.target.value)}
                  placeholder="h2h,spreads,totals"
                />
              </label>
              <label className="ingest-form__label">
                Books
                <input
                  className="input"
                  value={books}
                  onChange={(e) => setBooks(e.target.value)}
                  placeholder="draftkings,pinnacle"
                />
              </label>
            </div>
            <Button onClick={handleRun} disabled={isRunning}>
              {isRunning ? "Running..." : "Start Ingestion"}
            </Button>
          </div>
        </CardBody>
      </Card>

      {(logs.length > 0 || isRunning) && (
        <Card>
          <CardHeader>
            <span>Live Output</span>
            {progress && (
              <span className="ingest-progress-text">
                {progress.current}/{progress.total} — {progress.message}
              </span>
            )}
          </CardHeader>
          <CardBody>
            {progress && progress.total > 0 && (
              <div className="ingest-progress-bar">
                <div
                  className="ingest-progress-bar__fill"
                  style={{
                    width: `${(progress.current / progress.total) * 100}%`,
                  }}
                />
              </div>
            )}
            <div className="ingest-log">
              {logs.map((log, i) => (
                <div
                  key={i}
                  className={cn(
                    "ingest-log__entry",
                    log.level === "Error" && "ingest-log__entry--error",
                    log.level === "Warning" && "ingest-log__entry--warning",
                  )}
                >
                  <span className="ingest-log__time">
                    {new Date(log.timestamp).toLocaleTimeString()}
                  </span>
                  <span className="ingest-log__msg">{log.message}</span>
                </div>
              ))}
              <div ref={logEndRef} />
            </div>
            {summary && (
              <div className="ingest-summary">
                <Badge variant={summary.errorCount > 0 ? "danger" : "success"}>
                  {summary.errorCount > 0 ? "Completed with errors" : "Success"}
                </Badge>
                <span>
                  {summary.requestCount} requests, {summary.eventCount} events,{" "}
                  {summary.snapshotCount} snapshots
                </span>
                {summary.errorCount > 0 && (
                  <span className="ingest-summary__errors">
                    {summary.errorCount} error(s)
                  </span>
                )}
              </div>
            )}
          </CardBody>
        </Card>
      )}
    </>
  );
}

/* ----------------------------------------------------------------
   RECENT RUNS SECTION
   ---------------------------------------------------------------- */

function RecentRunsSection() {
  const { data: runs, isLoading } = useGetRunsQuery(20);

  return (
    <Card>
      <CardHeader>Recent Runs</CardHeader>
      {isLoading && (
        <CardBody>
          <p className="ingest-muted">Loading...</p>
        </CardBody>
      )}
      {!isLoading && runs && runs.length === 0 && (
        <CardBody>
          <p className="ingest-muted">No ingestion runs yet.</p>
        </CardBody>
      )}
      {runs && runs.length > 0 && (
        <Table>
          <thead>
            <tr>
              <Th>Type</Th>
              <Th>Status</Th>
              <Th>Started</Th>
              <Th align="right">Requests</Th>
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
                <Td align="right">{run.requestCount}</Td>
                <Td align="right">{run.eventCount}</Td>
                <Td align="right">{run.snapshotCount}</Td>
                <Td align="right">{run.errorCount}</Td>
              </Tr>
            ))}
          </tbody>
        </Table>
      )}
    </Card>
  );
}
