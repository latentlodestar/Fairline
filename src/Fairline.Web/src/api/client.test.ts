import { describe, it, expect, vi, beforeEach } from "vitest";
import { api } from "./client";

const mockFetch = vi.fn();
global.fetch = mockFetch;

describe("api client", () => {
  beforeEach(() => {
    mockFetch.mockReset();
  });

  it("getStatus fetches /api/status", async () => {
    const data = { version: "1.0.0", databaseConnected: true, timestamp: "2025-01-01T00:00:00Z" };
    mockFetch.mockResolvedValue({ ok: true, json: () => Promise.resolve(data) });

    const result = await api.getStatus();

    expect(mockFetch).toHaveBeenCalledWith("/api/status");
    expect(result).toEqual(data);
  });

  it("getProviders fetches /api/ingest/providers", async () => {
    mockFetch.mockResolvedValue({ ok: true, json: () => Promise.resolve([]) });

    const result = await api.getProviders();

    expect(mockFetch).toHaveBeenCalledWith("/api/ingest/providers");
    expect(result).toEqual([]);
  });

  it("throws on non-ok response", async () => {
    mockFetch.mockResolvedValue({ ok: false, status: 500, statusText: "Internal Server Error" });

    await expect(api.getStatus()).rejects.toThrow("HTTP 500: Internal Server Error");
  });
});
