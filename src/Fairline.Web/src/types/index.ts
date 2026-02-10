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
