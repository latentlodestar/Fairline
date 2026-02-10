import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { StatusPage } from "./StatusPage";
import { api } from "../api/client";
import type { ApiStatusResponse, ProviderInfo, ScenarioSummary } from "../types";

vi.mock("../api/client", () => ({
  api: {
    getStatus: vi.fn(),
    getProviders: vi.fn(),
    getScenarios: vi.fn(),
  },
}));

const mockStatus: ApiStatusResponse = {
  version: "1.0.0",
  databaseConnected: true,
  timestamp: new Date().toISOString(),
};

const mockProviders: ProviderInfo[] = [];
const mockScenarios: ScenarioSummary[] = [];

describe("StatusPage", () => {
  beforeEach(() => {
    vi.mocked(api.getStatus).mockResolvedValue(mockStatus);
    vi.mocked(api.getProviders).mockResolvedValue(mockProviders);
    vi.mocked(api.getScenarios).mockResolvedValue(mockScenarios);
  });

  it("renders loading state initially", () => {
    render(<StatusPage />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("renders dashboard after loading", async () => {
    render(<StatusPage />);
    await waitFor(() => {
      expect(screen.getByText("Dashboard")).toBeInTheDocument();
    });
  });

  it("shows database connected status", async () => {
    render(<StatusPage />);
    await waitFor(() => {
      expect(screen.getByText("Connected")).toBeInTheDocument();
    });
  });

  it("shows placeholder when no providers exist", async () => {
    render(<StatusPage />);
    await waitFor(() => {
      expect(screen.getByText("No providers configured yet")).toBeInTheDocument();
    });
  });

  it("shows error state on API failure", async () => {
    vi.mocked(api.getStatus).mockRejectedValue(new Error("Network error"));
    render(<StatusPage />);
    await waitFor(() => {
      expect(screen.getByText("Network error")).toBeInTheDocument();
    });
  });
});
