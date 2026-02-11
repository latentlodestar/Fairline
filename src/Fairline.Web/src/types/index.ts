export interface ApiStatusResponse {
  version: string;
  databaseConnected: boolean;
  timestamp: string;
}

export interface ProviderInfo {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
}

export interface ScenarioSummary {
  id: string;
  name: string;
  description: string | null;
  comparisonCount: number;
}

// Ingestion types

export interface SportCatalogEntry {
  providerSportKey: string;
  title: string;
  group: string;
  active: boolean;
  hasOutrights: boolean;
  normalizedSport: string;
  normalizedLeague: string;
  capturedAtUtc: string;
}

export interface TrackedLeagueInfo {
  id: string;
  provider: string;
  providerSportKey: string;
  enabled: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CatalogResponse {
  sports: SportCatalogEntry[];
  trackedLeagues: TrackedLeagueInfo[];
}

export interface CatalogRefreshResult {
  sportCount: number;
  sports: SportCatalogEntry[];
}

export interface IngestRunSummary {
  id: string;
  runType: string;
  status: string;
  startedAtUtc: string;
  completedAtUtc: string | null;
  requestCount: number;
  eventCount: number;
  snapshotCount: number;
  errorCount: number;
}

export interface IngestRunDetail extends IngestRunSummary {
  summary: string | null;
  logs: IngestLogEntry[];
}

export interface IngestLogEntry {
  level: string;
  message: string;
  createdAtUtc: string;
}

export interface RunIngestionRequest {
  windowHours?: number;
  regions?: string[];
  markets?: string[];
  books?: string[];
}

export interface SseLogEvent {
  level: string;
  message: string;
  timestamp: string;
}

export interface SseProgressEvent {
  current: number;
  total: number;
  message: string;
}

export interface SseSummaryEvent {
  requestCount: number;
  eventCount: number;
  snapshotCount: number;
  errorCount: number;
}

// Dashboard
export interface DashboardResponse {
  kpis: DashboardKpis;
  edges: EdgeRow[];
}
export interface DashboardKpis {
  eventCount: number;
  snapshotCount: number;
  bookCount: number;
  latestCaptureUtc: string | null;
}
export interface EdgeRow {
  sportEventId: string;
  homeTeam: string;
  awayTeam: string;
  sportKey: string;
  sportTitle: string;
  marketKey: string;
  outcomeName: string;
  bookmakerKey: string;
  bookmakerTitle: string;
  fairLine: number;
  bookLine: number;
  edgePct: number;
  signal: "value" | "tax" | "fair";
}

// Edge comparison model — one row per (event, market, selection)
export type EdgeSignal =
  | "value"
  | "tax"
  | "fair"
  | "no_baseline"
  | "no_target"
  | "line_mismatch";

export interface EdgeComparisonRow {
  eventId: string;
  homeTeam: string;
  awayTeam: string;
  sportKey: string;
  sportTitle: string;
  sportGroup: string;
  marketType: string;
  selectionKey: string;
  baselinePrice: number | null;
  baselinePoint: number | null;
  baselineDecimal: number | null;
  baselineBook: string | null;
  targetPrice: number | null;
  targetPoint: number | null;
  targetDecimal: number | null;
  targetBook: string;
  edgePct: number | null;
  signal: EdgeSignal;
  commenceTimeUtc: string;
  lastUpdatedUtc: string;
}

export interface EdgeComparisonsResponse {
  kpis: DashboardKpis;
  comparisons: EdgeComparisonRow[];
}

export interface EdgeComparisonsParams {
  baseline?: string;
  target?: string;
  showIncomplete?: boolean;
}
