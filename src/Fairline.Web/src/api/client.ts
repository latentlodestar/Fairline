import type { ApiStatusResponse, ProviderInfo, ScenarioSummary } from "../types";

const BASE_URL = "";

async function fetchJson<T>(path: string): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`);
  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  }
  return response.json() as Promise<T>;
}

export const api = {
  getStatus: () => fetchJson<ApiStatusResponse>("/api/status"),
  getProviders: () => fetchJson<ProviderInfo[]>("/api/ingest/providers"),
  getScenarios: () => fetchJson<ScenarioSummary[]>("/api/modeling/scenarios"),
};
