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
