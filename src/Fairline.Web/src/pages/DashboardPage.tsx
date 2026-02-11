import { useState, useMemo } from "react";
import { useGetDashboardQuery } from "../api/api";
import { KpiCard } from "../components/KpiCard";
import { Badge } from "../components/Badge";
import { Table, Th, Td, Tr } from "../components/Table";
import { Select, Input } from "../components/Input";
import type { EdgeRow } from "../types";

function formatMarket(key: string): string {
  switch (key) {
    case "h2h": return "ML";
    case "spreads": return "Spread";
    case "totals": return "Total";
    default: return key;
  }
}

function formatLine(row: EdgeRow, value: number): string {
  if (row.marketKey === "h2h") {
    return value >= 0 ? `+${Math.round(value)}` : `${Math.round(value)}`;
  }
  return String(value);
}

function formatEdge(pct: number): string {
  const sign = pct > 0 ? "+" : "";
  return `${sign}${pct.toFixed(2)}%`;
}

export function DashboardPage() {
  const { data, isLoading, error } = useGetDashboardQuery();
  const [sport, setSport] = useState("all");
  const [market, setMarket] = useState("all");
  const [book, setBook] = useState("all");
  const [search, setSearch] = useState("");

  const sports = useMemo(() => {
    if (!data) return [];
    const unique = [...new Set(data.edges.map((e) => e.sportKey))];
    return unique.sort();
  }, [data]);

  const markets = useMemo(() => {
    if (!data) return [];
    const unique = [...new Set(data.edges.map((e) => e.marketKey))];
    return unique.sort();
  }, [data]);

  const books = useMemo(() => {
    if (!data) return [];
    const unique = new Map<string, string>();
    for (const e of data.edges) {
      if (!unique.has(e.bookmakerKey)) {
        unique.set(e.bookmakerKey, e.bookmakerTitle);
      }
    }
    return [...unique.entries()].sort((a, b) => a[1].localeCompare(b[1]));
  }, [data]);

  const filtered = useMemo(() => {
    if (!data) return [];
    const q = search.toLowerCase();
    return data.edges.filter((e) => {
      if (sport !== "all" && e.sportKey !== sport) return false;
      if (market !== "all" && e.marketKey !== market) return false;
      if (book !== "all" && e.bookmakerKey !== book) return false;
      if (q && !`${e.homeTeam} ${e.awayTeam}`.toLowerCase().includes(q)) return false;
      return true;
    });
  }, [data, sport, market, book, search]);

  if (isLoading) {
    return (
      <div className="page">
        <p>Loading...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="page">
        <p>{"status" in error ? `Error: HTTP ${error.status}` : "Failed to load dashboard"}</p>
      </div>
    );
  }

  if (!data) return null;

  return (
    <div className="dashboard">
      <div className="kpi-strip">
        <KpiCard label="Events" value={data.kpis.eventCount} />
        <KpiCard label="Snapshots" value={data.kpis.snapshotCount.toLocaleString()} />
        <KpiCard label="Books" value={data.kpis.bookCount} />
        <KpiCard
          label="Last Capture"
          value={
            data.kpis.latestCaptureUtc
              ? new Date(data.kpis.latestCaptureUtc).toLocaleString()
              : "N/A"
          }
        />
      </div>

      <div className="filter-bar">
        <Select value={sport} onChange={(e) => setSport(e.target.value)}>
          <option value="all">All Sports</option>
          {sports.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </Select>
        <Select value={market} onChange={(e) => setMarket(e.target.value)}>
          <option value="all">All Markets</option>
          {markets.map((m) => (
            <option key={m} value={m}>{formatMarket(m)}</option>
          ))}
        </Select>
        <Select value={book} onChange={(e) => setBook(e.target.value)}>
          <option value="all">All Books</option>
          {books.map(([key, title]) => (
            <option key={key} value={key}>{title}</option>
          ))}
        </Select>
        <Input
          type="search"
          placeholder="Search events..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="card">
        <div className="card__header">
          <span>Edge Scanner</span>
          <Badge variant="neutral">{filtered.length} results</Badge>
        </div>
        {filtered.length === 0 ? (
          <div style={{ padding: "var(--space-4)" }}>
            <p className="placeholder">
              {data.edges.length === 0
                ? "No ingested odds data yet. Run an ingestion first."
                : "No edges match your filters."}
            </p>
          </div>
        ) : (
          <Table>
            <thead>
              <tr>
                <Th>Event</Th>
                <Th>Outcome</Th>
                <Th>Market</Th>
                <Th>Book</Th>
                <Th align="right">Fair Line</Th>
                <Th align="right">Book Line</Th>
                <Th align="right">Edge</Th>
                <Th>Signal</Th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row, i) => (
                <Tr
                  key={`${row.sportEventId}-${row.marketKey}-${row.outcomeName}-${row.bookmakerKey}`}
                  variant={
                    row.signal === "value"
                      ? "value"
                      : row.signal === "tax"
                        ? "tax"
                        : undefined
                  }
                >
                  <Td>{row.homeTeam} vs {row.awayTeam}</Td>
                  <Td>{row.outcomeName}</Td>
                  <Td>{formatMarket(row.marketKey)}</Td>
                  <Td>{row.bookmakerTitle}</Td>
                  <Td align="right">{formatLine(row, row.fairLine)}</Td>
                  <Td align="right">{formatLine(row, row.bookLine)}</Td>
                  <Td align="right">{formatEdge(row.edgePct)}</Td>
                  <Td>
                    <Badge
                      variant={
                        row.signal === "value"
                          ? "success"
                          : row.signal === "tax"
                            ? "danger"
                            : "neutral"
                      }
                    >
                      {row.signal === "value"
                        ? "Value"
                        : row.signal === "tax"
                          ? "Tax"
                          : "Fair"}
                    </Badge>
                  </Td>
                </Tr>
              ))}
            </tbody>
          </Table>
        )}
      </div>
    </div>
  );
}
