import { useState, useMemo } from "react";
import { useGetEdgeComparisonsQuery } from "../api/api";
import { Badge } from "../components/Badge";
import { Table, Th, Td, Tr } from "../components/Table";
import { Select, Input } from "../components/Input";
import { cn } from "../lib/cn";
import { formatDateTime } from "../lib/format";
import type { EdgeSignal } from "../types";

type SortColumn = "event" | "edge";
type SortDir = "asc" | "desc";

function formatPrice(
  marketType: string,
  price: number | null,
  point: number | null,
): string {
  if (price === null) return "\u2014";
  if (marketType === "h2h") {
    return price >= 0 ? `+${Math.round(price)}` : `${Math.round(price)}`;
  }
  // spreads / totals: show point + price
  const pointStr = point !== null ? String(point) : "";
  const priceStr = price >= 0 ? `+${Math.round(price)}` : `${Math.round(price)}`;
  return pointStr ? `${pointStr} (${priceStr})` : priceStr;
}

function formatEdge(pct: number | null): string {
  if (pct === null) return "\u2014";
  const sign = pct > 0 ? "+" : "";
  return `${sign}${pct.toFixed(2)}%`;
}

function signalVariant(signal: EdgeSignal): "success" | "danger" | "warning" | "neutral" {
  switch (signal) {
    case "value": return "success";
    case "tax": return "danger";
    case "no_baseline":
    case "no_target":
    case "line_mismatch": return "warning";
    default: return "neutral";
  }
}

function signalLabel(signal: EdgeSignal): string {
  switch (signal) {
    case "value": return "Value";
    case "tax": return "Tax";
    case "fair": return "Fair";
    case "no_baseline": return "No baseline";
    case "no_target": return "No target";
    case "line_mismatch": return "Line mismatch";
    default: return signal;
  }
}

function rowVariant(signal: EdgeSignal): "value" | "tax" | undefined {
  if (signal === "value") return "value";
  if (signal === "tax") return "tax";
  return undefined;
}

function shortEvent(home: string, away: string): string {
  return `${away} @ ${home}`;
}

function fullEvent(home: string, away: string): string {
  return `${home} vs ${away}`;
}

export function DashboardPage() {
  const { data, isLoading, error } = useGetEdgeComparisonsQuery();
  const [sportGroup, setSportGroup] = useState("all");
  const [league, setLeague] = useState("all");
  const [signal, setSignal] = useState("all");
  const [search, setSearch] = useState("");
  const [sortCol, setSortCol] = useState<SortColumn>("edge");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  function toggleSort(col: SortColumn) {
    if (sortCol === col) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortCol(col);
      setSortDir(col === "edge" ? "desc" : "asc");
    }
  }

  function sortIndicator(col: SortColumn): string {
    if (sortCol !== col) return "";
    return sortDir === "asc" ? " \u25B2" : " \u25BC";
  }

  const sportGroups = useMemo(() => {
    if (!data) return [];
    const unique = [...new Set(data.comparisons.map((c) => c.sportGroup))];
    return unique.sort();
  }, [data]);

  const leagues = useMemo(() => {
    if (!data) return [];
    const rows = sportGroup === "all"
      ? data.comparisons
      : data.comparisons.filter((c) => c.sportGroup === sportGroup);
    const seen = new Map<string, string>();
    for (const c of rows) {
      if (!seen.has(c.sportKey)) seen.set(c.sportKey, c.sportTitle);
    }
    return [...seen.entries()]
      .map(([key, title]) => ({ key, title }))
      .sort((a, b) => a.title.localeCompare(b.title));
  }, [data, sportGroup]);

  const filtered = useMemo(() => {
    if (!data) return [];
    const q = search.toLowerCase();
    const rows = data.comparisons.filter((c) => {
      if (sportGroup !== "all" && c.sportGroup !== sportGroup) return false;
      if (league !== "all" && c.sportKey !== league) return false;
      if (signal !== "all" && c.signal !== signal) return false;
      if (q && !`${c.homeTeam} ${c.awayTeam}`.toLowerCase().includes(q)) return false;
      return true;
    });

    const dir = sortDir === "asc" ? 1 : -1;
    rows.sort((a, b) => {
      if (sortCol === "event") {
        const aName = `${a.awayTeam} ${a.homeTeam}`.toLowerCase();
        const bName = `${b.awayTeam} ${b.homeTeam}`.toLowerCase();
        return aName < bName ? -dir : aName > bName ? dir : 0;
      }
      // edge: nulls always sort last
      const aEdge = a.edgePct ?? (dir > 0 ? Infinity : -Infinity);
      const bEdge = b.edgePct ?? (dir > 0 ? Infinity : -Infinity);
      return (aEdge - bEdge) * dir;
    });

    return rows;
  }, [data, sportGroup, league, signal, search, sortCol, sortDir]);

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
      <div className="filter-bar">
        <Select value={sportGroup} onChange={(e) => { setSportGroup(e.target.value); setLeague("all"); }}>
          <option value="all">All Sports</option>
          {sportGroups.map((g) => (
            <option key={g} value={g}>{g}</option>
          ))}
        </Select>
        <Select value={league} onChange={(e) => setLeague(e.target.value)} disabled={sportGroup === "all"}>
          <option value="all">All Leagues</option>
          {leagues.map((l) => (
            <option key={l.key} value={l.key}>{l.title}</option>
          ))}
        </Select>
        <Select value={signal} onChange={(e) => setSignal(e.target.value)}>
          <option value="all">All Signals</option>
          <option value="value">Value</option>
          <option value="tax">Tax</option>
          <option value="fair">Fair</option>
        </Select>
        <Input
          type="search"
          placeholder="Search events..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <div className="row-end">
          <span>Last Capture</span>
          <span>{
              data.kpis.latestCaptureUtc
                  ? formatDateTime(data.kpis.latestCaptureUtc)
                  : "N/A"
          }
          </span>
        </div>
      </div>

      <div className="card">
        <div className="card__header">
          <span>Edge Scanner</span>
          <Badge variant="neutral">{filtered.length} results</Badge>
        </div>
        {filtered.length === 0 ? (
          <div style={{ padding: "var(--space-4)" }}>
            <p className="placeholder">
              {data.comparisons.length === 0
                ? "No ingested odds data yet. Run an ingestion first."
                : "No edges match your filters."}
            </p>
          </div>
        ) : (
          <Table>
            <thead>
              <tr>
                <Th
                  className={cn("table__th--sortable", sortCol === "event" && "table__th--sorted")}
                  onClick={() => toggleSort("event")}
                >
                  Event{sortIndicator("event")}
                </Th>
                <Th>Selection</Th>
                <Th align="right">Pinnacle</Th>
                <Th align="right">DraftKings</Th>
                <Th
                  align="right"
                  className={cn("table__th--sortable", sortCol === "edge" && "table__th--sorted")}
                  onClick={() => toggleSort("edge")}
                >
                  Edge{sortIndicator("edge")}
                </Th>
                <Th>Signal</Th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row) => (
                <Tr
                  key={`${row.eventId}-${row.marketType}-${row.selectionKey}`}
                  variant={rowVariant(row.signal)}
                >
                  <Td>
                    <span
                      className="edge-event"
                      title={fullEvent(row.homeTeam, row.awayTeam)}
                    >
                      {shortEvent(row.homeTeam, row.awayTeam)}
                    </span>
                  </Td>
                  <Td>{row.selectionKey}</Td>
                  <Td align="right">
                    {row.baselineBook
                      ? formatPrice(row.marketType, row.baselinePrice, row.baselinePoint)
                      : <span className="edge-missing">N/A</span>}
                  </Td>
                  <Td align="right">
                    {formatPrice(row.marketType, row.targetPrice, row.targetPoint)}
                  </Td>
                  <Td align="right">
                    <span className={cn(
                      "edge-pct",
                      row.edgePct !== null && row.edgePct >= 1 && "edge-pct--value",
                      row.edgePct !== null && row.edgePct <= -1 && "edge-pct--tax",
                    )}>
                      {formatEdge(row.edgePct)}
                    </span>
                  </Td>
                  <Td>
                    <Badge variant={signalVariant(row.signal)}>
                      {signalLabel(row.signal)}
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
