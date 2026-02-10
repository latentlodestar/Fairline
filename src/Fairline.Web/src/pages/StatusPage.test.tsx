import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import type { ApiStatusResponse } from "../types";

const mockStatus: ApiStatusResponse = {
  version: "1.0.0",
  databaseConnected: true,
  timestamp: new Date().toISOString(),
};

const mockUseGetStatusQuery = vi.fn();
const mockUseGetProvidersQuery = vi.fn();
const mockUseGetScenariosQuery = vi.fn();

vi.mock("../api/api", () => ({
  useGetStatusQuery: (...args: unknown[]) => mockUseGetStatusQuery(...args),
  useGetProvidersQuery: (...args: unknown[]) => mockUseGetProvidersQuery(...args),
  useGetScenariosQuery: (...args: unknown[]) => mockUseGetScenariosQuery(...args),
}));

function setDefaults(overrides?: {
  status?: Partial<ReturnType<typeof mockUseGetStatusQuery>>;
  providers?: Partial<ReturnType<typeof mockUseGetProvidersQuery>>;
  scenarios?: Partial<ReturnType<typeof mockUseGetScenariosQuery>>;
}) {
  mockUseGetStatusQuery.mockReturnValue({
    data: mockStatus,
    isLoading: false,
    error: undefined,
    ...overrides?.status,
  });
  mockUseGetProvidersQuery.mockReturnValue({
    data: [],
    isLoading: false,
    error: undefined,
    ...overrides?.providers,
  });
  mockUseGetScenariosQuery.mockReturnValue({
    data: [],
    isLoading: false,
    error: undefined,
    ...overrides?.scenarios,
  });
}

// Import after mocks are set up
import { StatusPage } from "./StatusPage";

describe("StatusPage", () => {
  it("renders loading state initially", () => {
    setDefaults({
      status: { data: undefined, isLoading: true },
      providers: { data: undefined, isLoading: true },
      scenarios: { data: undefined, isLoading: true },
    });
    render(<StatusPage />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders dashboard after loading", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("System Status")).toBeInTheDocument();
  });

  it("shows database connected status", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("Connected")).toBeInTheDocument();
  });

  it("shows placeholder when no providers exist", () => {
    setDefaults();
    render(<StatusPage />);
    expect(screen.getByText("No providers configured yet")).toBeInTheDocument();
  });

  it("shows error state on API failure", () => {
    setDefaults({
      status: { data: undefined, error: { message: "Network error" } },
    });
    render(<StatusPage />);
    expect(screen.getByText("Network error")).toBeInTheDocument();
  });
});
