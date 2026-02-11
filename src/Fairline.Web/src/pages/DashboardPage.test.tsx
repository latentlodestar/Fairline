import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { EdgeComparisonsResponse } from "../types";

const mockComparisons: EdgeComparisonsResponse = {
  kpis: {
    eventCount: 2,
    snapshotCount: 8,
    bookCount: 3,
    latestCaptureUtc: new Date().toISOString(),
  },
  comparisons: [
    {
      eventId: "evt-1",
      homeTeam: "Arsenal",
      awayTeam: "Wigan",
      sportKey: "soccer_epl",
      sportTitle: "EPL",
      marketType: "h2h",
      selectionKey: "Draw",
      baselinePrice: 300,
      baselinePoint: null,
      baselineDecimal: 4.0,
      baselineBook: "pinnacle",
      targetPrice: 250,
      targetPoint: null,
      targetDecimal: 3.5,
      targetBook: "draftkings",
      edgePct: -3.7,
      signal: "tax",
      lastUpdatedUtc: new Date().toISOString(),
    },
    {
      eventId: "evt-1",
      homeTeam: "Arsenal",
      awayTeam: "Wigan",
      sportKey: "soccer_epl",
      sportTitle: "EPL",
      marketType: "h2h",
      selectionKey: "Arsenal",
      baselinePrice: -150,
      baselinePoint: null,
      baselineDecimal: 1.6667,
      baselineBook: "pinnacle",
      targetPrice: -130,
      targetPoint: null,
      targetDecimal: 1.7692,
      targetBook: "draftkings",
      edgePct: 3.5,
      signal: "value",
      lastUpdatedUtc: new Date().toISOString(),
    },
  ],
};

const mockUseGetEdgeComparisonsQuery = vi.fn();

vi.mock("../api/api", () => ({
  useGetEdgeComparisonsQuery: (...args: unknown[]) =>
    mockUseGetEdgeComparisonsQuery(...args),
}));

function setDefaults(
  overrides?: Partial<ReturnType<typeof mockUseGetEdgeComparisonsQuery>>,
) {
  mockUseGetEdgeComparisonsQuery.mockReturnValue({
    data: mockComparisons,
    isLoading: false,
    error: undefined,
    ...overrides,
  });
}

// Import after mocks are set up
import { DashboardPage } from "./DashboardPage";

describe("DashboardPage", () => {
  it("renders loading state", () => {
    setDefaults({ data: undefined, isLoading: true });
    render(<DashboardPage />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders error state", () => {
    setDefaults({ data: undefined, error: { status: 500 } });
    render(<DashboardPage />);
    expect(screen.getByText("Error: HTTP 500")).toBeInTheDocument();
  });

  it("renders empty state when no comparisons", () => {
    setDefaults({
      data: { ...mockComparisons, comparisons: [] },
    });
    render(<DashboardPage />);
    expect(
      screen.getByText("No ingested odds data yet. Run an ingestion first."),
    ).toBeInTheDocument();
  });

  it("renders KPI cards", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Events")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("Books")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("renders Edge Scanner header", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Edge Scanner")).toBeInTheDocument();
    expect(screen.getByText("2 results")).toBeInTheDocument();
  });

  it("renders one row per selection (no duplicate events)", () => {
    setDefaults();
    render(<DashboardPage />);
    // Both rows are for same event but different selections
    expect(screen.getAllByText(/Wigan @ Arsenal/)).toHaveLength(2);
    expect(screen.getByText("Draw")).toBeInTheDocument();
    expect(screen.getByText("Arsenal")).toBeInTheDocument();
  });

  it("shows Value badge for positive edge", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Value", { selector: ".badge" })).toBeInTheDocument();
  });

  it("shows Tax badge for negative edge", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Tax", { selector: ".badge" })).toBeInTheDocument();
  });

  it("renders comparison columns (Pinnacle + DraftKings)", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Pinnacle")).toBeInTheDocument();
    expect(screen.getByText("DraftKings")).toBeInTheDocument();
  });

  it("formats moneyline prices with sign", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("+300")).toBeInTheDocument();
    expect(screen.getByText("+250")).toBeInTheDocument();
    expect(screen.getByText("-150")).toBeInTheDocument();
    expect(screen.getByText("-130")).toBeInTheDocument();
  });

  it("formats edge percentage", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("-3.70%")).toBeInTheDocument();
    expect(screen.getByText("+3.50%")).toBeInTheDocument();
  });

  it("shows no_baseline warning for rows without baseline", () => {
    setDefaults({
      data: {
        ...mockComparisons,
        comparisons: [
          {
            ...mockComparisons.comparisons[0],
            baselineBook: null,
            baselinePrice: null,
            baselinePoint: null,
            baselineDecimal: null,
            edgePct: null,
            signal: "no_baseline" as const,
          },
        ],
      },
    });
    render(<DashboardPage />);
    expect(screen.getByText("No baseline")).toBeInTheDocument();
    expect(screen.getByText("N/A")).toBeInTheDocument();
  });
});
