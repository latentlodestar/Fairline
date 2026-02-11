import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { ApiStatusResponse, IngestRunSummary, DashboardResponse } from "../types";

const mockStatus: ApiStatusResponse = {
  version: "1.0.0",
  databaseConnected: true,
  timestamp: new Date().toISOString(),
};

const mockRun: IngestRunSummary = {
  id: "run-1",
  runType: "GapFill",
  status: "Completed",
  startedAtUtc: new Date().toISOString(),
  completedAtUtc: new Date().toISOString(),
  requestCount: 5,
  eventCount: 10,
  snapshotCount: 20,
  errorCount: 0,
};

const mockDashboard: DashboardResponse = {
  kpis: {
    eventCount: 10,
    snapshotCount: 20,
    bookCount: 3,
    latestCaptureUtc: new Date().toISOString(),
  },
  edges: [],
};

const mockUseGetStatusQuery = vi.fn();
const mockUseGetRunsQuery = vi.fn();
const mockUseGetDashboardQuery = vi.fn();

vi.mock("../api/api", () => ({
  useGetStatusQuery: (...args: unknown[]) => mockUseGetStatusQuery(...args),
  useGetRunsQuery: (...args: unknown[]) => mockUseGetRunsQuery(...args),
  useGetDashboardQuery: (...args: unknown[]) => mockUseGetDashboardQuery(...args),
}));

function setDefaults(overrides?: {
  status?: Partial<ReturnType<typeof mockUseGetStatusQuery>>;
  runs?: Partial<ReturnType<typeof mockUseGetRunsQuery>>;
  dashboard?: Partial<ReturnType<typeof mockUseGetDashboardQuery>>;
}) {
  mockUseGetStatusQuery.mockReturnValue({
    data: mockStatus,
    isLoading: false,
    error: undefined,
    ...overrides?.status,
  });
  mockUseGetRunsQuery.mockReturnValue({
    data: [mockRun],
    isLoading: false,
    error: undefined,
    ...overrides?.runs,
  });
  mockUseGetDashboardQuery.mockReturnValue({
    data: mockDashboard,
    isLoading: false,
    error: undefined,
    ...overrides?.dashboard,
  });
}

// Import after mocks are set up
import { StatusPage } from "./StatusPage";

describe("StatusPage", () => {
  it("renders loading state initially", () => {
    setDefaults({
      status: { data: undefined, isLoading: true },
      runs: { data: undefined, isLoading: true },
    });
    render(<StatusPage />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders page after loading", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("System Status")).toBeInTheDocument();
  });

  it("shows database connected status", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("Connected")).toBeInTheDocument();
  });

  it("shows placeholder when no runs exist", () => {
    setDefaults({ runs: { data: [] } });
    render(<StatusPage />);
    expect(screen.getByText("No ingestion runs yet.")).toBeInTheDocument();
  });

  it("shows run data in table", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("GapFill")).toBeInTheDocument();
    expect(screen.getByText("Completed", { selector: ".badge" })).toBeInTheDocument();
  });

  it("shows error state on API failure", () => {
    setDefaults({
      status: { data: undefined, error: { message: "Network error" } },
    });
    render(<StatusPage />);
    expect(screen.getByText("Network error")).toBeInTheDocument();
  });
});
