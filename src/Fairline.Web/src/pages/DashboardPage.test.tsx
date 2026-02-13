import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { EdgeComparisonsResponse } from "../types";

const now = new Date().toISOString();

const mockComparisons: EdgeComparisonsResponse = {
  kpis: {
    eventCount: 2,
    snapshotCount: 8,
    bookCount: 3,
    latestCaptureUtc: now,
  },
  comparisons: [
    {
      eventId: "evt-1",
      homeTeam: "Arsenal",
      awayTeam: "Wigan",
      sportKey: "soccer_epl",
      sportTitle: "EPL",
      sportGroup: "Soccer",
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
      commenceTimeUtc: "2025-06-15T15:00:00Z",
      lastUpdatedUtc: now,
    },
    {
      eventId: "evt-1",
      homeTeam: "Arsenal",
      awayTeam: "Wigan",
      sportKey: "soccer_epl",
      sportTitle: "EPL",
      sportGroup: "Soccer",
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
      commenceTimeUtc: "2025-06-15T15:00:00Z",
      lastUpdatedUtc: now,
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

  it("renders Edge Scanner header with event count", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("Edge Scanner")).toBeInTheDocument();
    expect(screen.getByText("1 event")).toBeInTheDocument();
  });

  it("renders outer table columns for event-level data", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText(/^Event/)).toBeInTheDocument();
    expect(screen.getByText("Competition")).toBeInTheDocument();
    expect(screen.getByText("Start Time")).toBeInTheDocument();
    expect(screen.getByText("Signals")).toBeInTheDocument();
  });

  it("groups rows by event — event name appears once", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getAllByText(/Wigan @ Arsenal/)).toHaveLength(1);
  });

  it("shows best edge and signal counts on group row", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.getByText("+3.50%")).toBeInTheDocument();
    expect(screen.getByText("1V")).toBeInTheDocument();
    expect(screen.getByText("1T")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
  });

  it("does not show inner table content when collapsed", () => {
    setDefaults();
    render(<DashboardPage />);
    expect(screen.queryByText("Draw")).not.toBeInTheDocument();
    // Inner table headers should not be present
    expect(screen.queryByText("Selection")).not.toBeInTheDocument();
  });

  it("expands to show inner table with its own headers on click", () => {
    setDefaults();
    render(<DashboardPage />);

    fireEvent.click(screen.getByText(/Wigan @ Arsenal/));

    // Inner table headers
    expect(screen.getByText("Selection")).toBeInTheDocument();
    expect(screen.getByText("Market")).toBeInTheDocument();
    expect(screen.getByText("Pinnacle")).toBeInTheDocument();
    expect(screen.getByText("DraftKings")).toBeInTheDocument();
    // Inner table rows
    expect(screen.getByText("Draw")).toBeInTheDocument();
    expect(screen.getAllByText("Moneyline")).toHaveLength(2);
  });

  it("shows Value and Tax badges on expanded inner rows", () => {
    setDefaults();
    render(<DashboardPage />);

    fireEvent.click(screen.getByText(/Wigan @ Arsenal/));

    expect(screen.getByText("Value", { selector: ".badge" })).toBeInTheDocument();
    expect(screen.getByText("Tax", { selector: ".badge" })).toBeInTheDocument();
  });

  it("formats moneyline prices when expanded", () => {
    setDefaults();
    render(<DashboardPage />);

    fireEvent.click(screen.getByText(/Wigan @ Arsenal/));

    expect(screen.getByText("+300")).toBeInTheDocument();
    expect(screen.getByText("+250")).toBeInTheDocument();
    expect(screen.getByText("-150")).toBeInTheDocument();
    expect(screen.getByText("-130")).toBeInTheDocument();
  });

  it("formats edge percentages in inner rows", () => {
    setDefaults();
    render(<DashboardPage />);

    fireEvent.click(screen.getByText(/Wigan @ Arsenal/));

    expect(screen.getByText("-3.70%")).toBeInTheDocument();
  });

  it("collapses expanded group on second click", () => {
    setDefaults();
    render(<DashboardPage />);

    const header = screen.getByText(/Wigan @ Arsenal/);
    fireEvent.click(header);
    expect(screen.getByText("Draw")).toBeInTheDocument();

    fireEvent.click(header);
    expect(screen.queryByText("Draw")).not.toBeInTheDocument();
  });

  it("Expand All / Collapse All buttons work", () => {
    setDefaults();
    render(<DashboardPage />);

    fireEvent.click(screen.getByText("Expand All"));
    expect(screen.getByText("Draw")).toBeInTheDocument();

    fireEvent.click(screen.getByText("Collapse All"));
    expect(screen.queryByText("Draw")).not.toBeInTheDocument();
  });

  it("expander button has aria-expanded attribute", () => {
    setDefaults();
    render(<DashboardPage />);

    const expander = screen.getByRole("button", { expanded: false });
    expect(expander).toHaveAttribute("aria-controls", "lines-evt-1");

    fireEvent.click(expander);
    expect(expander).toHaveAttribute("aria-expanded", "true");
  });

  it("shows no_baseline warning in inner table", () => {
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

    fireEvent.click(screen.getByText(/Wigan @ Arsenal/));

    expect(screen.getByText("No baseline")).toBeInTheDocument();
    expect(screen.getByText("N/A")).toBeInTheDocument();
  });
});
