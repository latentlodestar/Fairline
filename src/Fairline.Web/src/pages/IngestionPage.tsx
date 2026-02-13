import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Button } from "../components/Button";
import { Card, CardBody, CardHeader } from "../components/Card";
import { Badge } from "../components/Badge";
import { Dialog } from "../components/Dialog";
import { Table, Th, Td, Tr } from "../components/Table";
import { cn } from "../lib/cn";
import { formatDateTime, formatTime } from "../lib/format";
import {
  api,
  useGetCatalogQuery,
  useRefreshCatalogMutation,
  useToggleTrackedLeagueMutation,
  useRunIngestionMutation,
  useCancelRunMutation,
  useGetRunsQuery,
  useGetRunDetailQuery,
} from "../api/api";
import { useAppDispatch } from "../store";
import type {
  SseLogEvent,
  SseProgressEvent,
  SseSummaryEvent,
} from "../types";

/* ----------------------------------------------------------------
   INGESTION PRESETS (localStorage)
   ---------------------------------------------------------------- */

interface IngestionPreset {
  name: string;
  windowHours: number;
  regions: string;
  markets: string;
  books?: string;
}

const PRESETS_KEY = "fairline:ingestion-presets";
const ACTIVE_PRESET_KEY = "fairline:ingestion-active-preset";

const DEFAULT_VALUES: Omit<IngestionPreset, "name"> = {
  windowHours: 72,
  regions: "us",
  markets: "h2h,spreads,totals,outrights",
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
   MAIN PAGE
   ---------------------------------------------------------------- */

export function IngestionPage() {
  // --- Preset state ---
  const [presets, setPresets] = useState(loadPresets);
  const [activePresetName, setActivePresetName] = useState(loadActivePresetName);

  const initial =
    presets.find((p) => p.name === activePresetName) ?? DEFAULT_VALUES;
  const [regions, setRegions] = useState(initial.regions);
  const [markets, setMarkets] = useState(initial.markets);
  const [windowHours, setWindowHours] = useState(initial.windowHours);

  // --- SSE / run state ---
  const [runIngestion] = useRunIngestionMutation();
  const [cancelRun] = useCancelRunMutation();
  const dispatch = useAppDispatch();

  const [activeRunId, setActiveRunId] = useState<string | null>(null);
  const [logs, setLogs] = useState<SseLogEvent[]>([]);
  const [progress, setProgress] = useState<SseProgressEvent | null>(null);
  const [summary, setSummary] = useState<SseSummaryEvent | null>(null);
  const [isRunning, setIsRunning] = useState(false);

  // --- Log modal state ---
  const [logModalOpen, setLogModalOpen] = useState(false);
  const [selectedRunId, setSelectedRunId] = useState<string | null>(null);

  // --- Preset helpers ---
  const applyPreset = (name: string | null) => {
    const preset = presets.find((p) => p.name === name);
    const values = preset ?? DEFAULT_VALUES;
    setWindowHours(values.windowHours);
    setRegions(values.regions);
    setMarkets(values.markets);
    setActivePresetName(name);
    saveActivePresetName(name);
  };

  const handleSavePreset = () => {
    const name = window.prompt("Preset name:", activePresetName ?? "");
    if (!name?.trim()) return;
    const trimmed = name.trim();
    const preset: IngestionPreset = {
      name: trimmed,
      windowHours,
      regions,
      markets,
    };
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

  // --- SSE effect ---
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

  // --- Start ingestion ---
  const handleRun = async () => {
    setLogs([]);
    setProgress(null);
    setSummary(null);
    setIsRunning(true);
    setSelectedRunId(null);
    setLogModalOpen(true);

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
    }).unwrap();

    setActiveRunId(result.runId);
  };

  // --- Open historical log ---
  const openHistoricalLog = (runId: string) => {
    setSelectedRunId(runId);
    setLogModalOpen(true);
  };

  const closeLogModal = () => {
    setLogModalOpen(false);
    // Don't clear selectedRunId/activeRunId immediately so Dialog closing animation can finish
  };

  return (
    <div className="page">
      <h1 className="section-title">Ingestion</h1>

      <CatalogSection
        onRun={handleRun}
        isRunning={isRunning}
        presets={presets}
        activePresetName={activePresetName}
        applyPreset={applyPreset}
        handleSavePreset={handleSavePreset}
        handleDeletePreset={handleDeletePreset}
        windowHours={windowHours}
        setWindowHours={setWindowHours}
        regions={regions}
        setRegions={setRegions}
        markets={markets}
        setMarkets={setMarkets}
      />

      <RecentRunsSection onSelectRun={openHistoricalLog} />

      <IngestionLogDialog
        open={logModalOpen}
        onClose={closeLogModal}
        // Live mode
        isLive={selectedRunId === null}
        isRunning={isRunning}
        logs={logs}
        progress={progress}
        summary={summary}
        onCancel={(runId: string) => { cancelRun(runId).then(() => dispatch(api.util.invalidateTags(["Runs"]))); closeLogModal(); }}
        activeRunId={activeRunId}
        // Historical mode
        selectedRunId={selectedRunId}
      />
    </div>
  );
}

/* ----------------------------------------------------------------
   CATALOG SECTION (collapsible, with action menu + filter bar)
   ---------------------------------------------------------------- */

interface CatalogSectionProps {
  onRun: () => void;
  isRunning: boolean;
  presets: IngestionPreset[];
  activePresetName: string | null;
  applyPreset: (name: string | null) => void;
  handleSavePreset: () => void;
  handleDeletePreset: () => void;
  windowHours: number;
  setWindowHours: (v: number) => void;
  regions: string;
  setRegions: (v: string) => void;
  markets: string;
  setMarkets: (v: string) => void;
}

function CatalogSection({
  onRun,
  isRunning,
  presets,
  activePresetName,
  applyPreset,
  handleSavePreset,
  handleDeletePreset,
  windowHours,
  setWindowHours,
  regions,
  setRegions,
  markets,
  setMarkets,
}: CatalogSectionProps) {
  const { data: catalog, isLoading } = useGetCatalogQuery();
  const [refreshCatalog, { isLoading: isRefreshing }] =
    useRefreshCatalogMutation();
  const [toggleLeague] = useToggleTrackedLeagueMutation();

  const [inSeasonOnly, setInSeasonOnly] = useState(false);
  const [collapsed, setCollapsed] = useState(true);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu on outside click
  useEffect(() => {
    if (!menuOpen) return;
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [menuOpen]);

  const trackedMap = new Map(
    catalog?.trackedLeagues?.map((t) => [t.providerSportKey, t.enabled]) ?? [],
  );

  const enabledLeagues = useMemo(
    () => catalog?.trackedLeagues?.filter((t) => t.enabled) ?? [],
    [catalog],
  );

  const enabledLeagueCount = enabledLeagues.length;

  const estimatedRequests = useMemo(() => {
    const parsed = markets.split(",").map((s) => s.trim()).filter(Boolean);
    const marketCount = parsed.length;
    const hasOutrights = parsed.includes("outrights");

    if (!hasOutrights) return enabledLeagueCount * marketCount;

    const outrightsMap = new Map(
      catalog?.sports?.map((s) => [s.providerSportKey, s.hasOutrights]) ?? [],
    );

    let skipped = 0;
    for (const league of enabledLeagues) {
      if (!outrightsMap.get(league.providerSportKey)) skipped += 1;
    }
    return enabledLeagueCount * marketCount - skipped;
  }, [markets, enabledLeagues, enabledLeagueCount, catalog]);

  const sports = inSeasonOnly
    ? (catalog?.sports ?? []).filter((s) => s.active)
    : (catalog?.sports ?? []);

  const grouped = new Map<string, typeof sports>();
  for (const sport of sports) {
    const group = sport.group;
    if (!grouped.has(group)) grouped.set(group, []);
    grouped.get(group)!.push(sport);
  }

  return (
    <Card>
      {/* Top-level bar: always visible */}
      <div className="ingest-topbar">
        <div className="ingest-topbar__left">
          <button
            className={cn(
              "ingest-collapse-btn",
              !collapsed && "ingest-collapse-btn--open",
            )}
            onClick={() => setCollapsed((v) => !v)}
            aria-label={collapsed ? "Expand settings" : "Collapse settings"}
          >
            ▸
          </button>
          <span className="ingest-topbar__title">Ingestion Settings</span>

          <div className="ingest-cost-estimate">
            <span className="ingest-cost-estimate__value">{estimatedRequests}</span>
            <span className="ingest-cost-estimate__label">
              API requests
              <span className="ingest-cost-estimate__breakdown">
                {enabledLeagueCount} league{enabledLeagueCount !== 1 ? "s" : ""}
              </span>
            </span>
          </div>
        </div>

        <div className="ingest-topbar__right">
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

          <div className="ingest-action-wrap" ref={menuRef}>
            <Button
              className="ingest-action-btn"
              onClick={() => setMenuOpen((v) => !v)}
              aria-label="Actions"
            >
              ⋮
            </Button>
            {menuOpen && (
              <div className="ingest-action-menu">
                <button
                  className="ingest-action-menu__item"
                  disabled={isRunning}
                  onClick={() => {
                    onRun();
                    setMenuOpen(false);
                  }}
                >
                  {isRunning ? "Running..." : "Start Ingestion"}
                </button>
                <button
                  className="ingest-action-menu__item"
                  disabled={isRefreshing}
                  onClick={() => {
                    refreshCatalog();
                    setMenuOpen(false);
                  }}
                >
                  {isRefreshing ? "Refreshing..." : "Refresh Catalog"}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Collapsible body */}
      {!collapsed && (
        <CardBody>
          {/* Parameters section */}
          <div className="ingest-section">
            <div className="ingest-section__title">Parameters</div>
            <div className="ingest-params">
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
            </div>
          </div>

          {/* Sports section */}
          <div className="ingest-section">
            <div className="ingest-section__header">
              <div className="ingest-section__title">Sports</div>
              <label className="ingest-toggle">
                <input
                  type="checkbox"
                  checked={inSeasonOnly}
                  onChange={() => setInSeasonOnly((v) => !v)}
                />
                <span className="ingest-toggle__label">In Season Only</span>
              </label>
            </div>

            {isLoading && <p className="ingest-muted">Loading catalog...</p>}
            {!isLoading && grouped.size === 0 && (
              <p className="ingest-muted">
                No sports in catalog. Use the ⋮ menu to refresh from the Odds API.
              </p>
            )}
            {[...grouped.entries()].map(([group, sports]) => (
              <div key={group} className="ingest-catalog-group">
                <div className="ingest-catalog-group__title">{group}</div>
                <div className="ingest-catalog-group__items">
                  {sports.map((sport) => {
                    const tracked =
                      trackedMap.get(sport.providerSportKey) ?? false;
                    return (
                      <label
                        key={sport.providerSportKey}
                        className="ingest-toggle"
                      >
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
          </div>
        </CardBody>
      )}
    </Card>
  );
}

/* ----------------------------------------------------------------
   RECENT RUNS SECTION (clickable rows)
   ---------------------------------------------------------------- */

function RecentRunsSection({
  onSelectRun,
}: {
  onSelectRun: (runId: string) => void;
}) {
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
              <Tr
                key={run.id}
                className="table__row--clickable"
                onClick={() => onSelectRun(run.id)}
              >
                <Td>
                  <Badge variant="neutral">{run.runType}</Badge>
                </Td>
                <Td>
                  <Badge variant={statusBadgeVariant(run.status)}>
                    {run.status}
                  </Badge>
                </Td>
                <Td>{formatDateTime(run.startedAtUtc)}</Td>
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

/* ----------------------------------------------------------------
   INGESTION LOG DIALOG (live SSE or historical)
   ---------------------------------------------------------------- */

interface IngestionLogDialogProps {
  open: boolean;
  onClose: () => void;
  // Live mode
  isLive: boolean;
  isRunning: boolean;
  logs: SseLogEvent[];
  progress: SseProgressEvent | null;
  summary: SseSummaryEvent | null;
  onCancel: (runId: string) => void;
  activeRunId: string | null;
  // Historical mode
  selectedRunId: string | null;
}

function IngestionLogDialog({
  open,
  onClose,
  isLive,
  isRunning,
  logs,
  progress,
  summary,
  onCancel,
  activeRunId,
  selectedRunId,
}: IngestionLogDialogProps) {
  const logEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = useCallback(() => {
    logEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  // Auto-scroll on new live logs
  useEffect(() => {
    if (isLive) scrollToBottom();
  }, [logs, isLive, scrollToBottom]);

  if (isLive) {
    return (
      <Dialog
        open={open}
        onClose={onClose}
        className="dialog--wide"
        title={
          <>
            <span>Ingestion Log</span>
            {isRunning ? (
              <Badge variant="warning">Running</Badge>
            ) : summary ? (
              <Badge variant={summary.errorCount > 0 ? "danger" : "success"}>
                {summary.errorCount > 0 ? "Failed" : "Completed"}
              </Badge>
            ) : null}
          </>
        }
        footer={
          <>
            <div className="ingest-summary">
              {summary && (
                <>
                  <span>
                    {summary.requestCount} requests, {summary.eventCount} events,{" "}
                    {summary.snapshotCount} snapshots
                  </span>
                  {summary.errorCount > 0 && (
                    <span className="ingest-summary__errors">
                      {summary.errorCount} error(s)
                    </span>
                  )}
                </>
              )}
            </div>
            <div className="flex items-center gap-2">
              {isRunning && activeRunId && (
                <Button size="sm" variant="danger" onClick={() => onCancel(activeRunId)}>
                  Cancel
                </Button>
              )}
              <Button size="sm" variant="secondary" onClick={onClose}>
                Close
              </Button>
            </div>
          </>
        }
      >
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
        {progress && (
          <p className="ingest-progress-text" style={{ marginBottom: "0.5rem" }}>
            {progress.current}/{progress.total} — {progress.message}
          </p>
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
                {formatTime(log.timestamp)}
              </span>
              <span className="ingest-log__msg">{log.message}</span>
            </div>
          ))}
          <div ref={logEndRef} />
        </div>
      </Dialog>
    );
  }

  // Historical mode
  return (
    <Dialog
      open={open}
      onClose={onClose}
      className="dialog--wide"
      title={
        selectedRunId ? (
          <HistoricalLogTitle runId={selectedRunId} />
        ) : (
          <span>Ingestion Log</span>
        )
      }
      footer={
        selectedRunId ? (
          <HistoricalLogFooter runId={selectedRunId} onCancel={onCancel} onClose={onClose} />
        ) : (
          <Button size="sm" variant="secondary" onClick={onClose}>
            Close
          </Button>
        )
      }
    >
      {selectedRunId && <HistoricalLogContent runId={selectedRunId} />}
    </Dialog>
  );
}

function statusBadgeVariant(status: string): "success" | "danger" | "neutral" | "warning" {
  switch (status) {
    case "Completed": return "success";
    case "Failed": return "danger";
    case "Cancelled": return "neutral";
    default: return "warning";
  }
}

function HistoricalLogTitle({ runId }: { runId: string }) {
  const { data: detail } = useGetRunDetailQuery(runId);

  return (
    <>
      <span>Ingestion Log</span>
      {detail && <Badge variant={statusBadgeVariant(detail.status)}>{detail.status}</Badge>}
    </>
  );
}

function HistoricalLogFooter({ runId, onCancel, onClose }: { runId: string; onCancel: (runId: string) => void; onClose: () => void }) {
  const { data: detail } = useGetRunDetailQuery(runId);

  return (
    <>
      <div className="ingest-summary">
        {detail && (
          <>
            <span>
              {detail.requestCount} requests, {detail.eventCount} events,{" "}
              {detail.snapshotCount} snapshots
            </span>
            {detail.errorCount > 0 && (
              <span className="ingest-summary__errors">
                {detail.errorCount} error(s)
              </span>
            )}
          </>
        )}
      </div>
      <div className="flex items-center gap-2">
        {detail?.status === "Running" && (
          <Button size="sm" variant="danger" onClick={() => onCancel(runId)}>
            Cancel
          </Button>
        )}
        <Button size="sm" variant="secondary" onClick={onClose}>
          Close
        </Button>
      </div>
    </>
  );
}

function HistoricalLogContent({ runId }: { runId: string }) {
  const { data: detail, isLoading } = useGetRunDetailQuery(runId);

  if (isLoading) {
    return <p className="ingest-muted">Loading logs...</p>;
  }

  if (!detail) {
    return <p className="ingest-muted">Run not found.</p>;
  }

  return (
    <>
      <div className="ingest-log">
        {detail.logs.map((log, i) => (
          <div
            key={i}
            className={cn(
              "ingest-log__entry",
              log.level === "Error" && "ingest-log__entry--error",
              log.level === "Warning" && "ingest-log__entry--warning",
            )}
          >
            <span className="ingest-log__time">
              {formatTime(log.createdAtUtc)}
            </span>
            <span className="ingest-log__msg">{log.message}</span>
          </div>
        ))}
      </div>
    </>
  );
}
