import { useState, useMemo, Fragment } from "react";
import { useGetEdgeComparisonsQuery } from "../api/api";
import { Badge } from "../components/Badge";
import { Table, Th, Td } from "../components/Table";
import { Select, Input } from "../components/Input";
import { cn } from "../lib/cn";
import { formatDateTime } from "../lib/format";
import type { EdgeComparisonRow, EdgeSignal } from "../types";

type SortColumn = "event" | "edge";
type SortDir = "asc" | "desc";

interface EventGroup {
  eventId: string;
  homeTeam: string;
  awayTeam: string;
  sportTitle: string;
  sportGroup: string;
  commenceTimeUtc: string;
  rows: EdgeComparisonRow[];
  bestEdge: number | null;
  valueCount: number;
  taxCount: number;
}

const OUTER_COL_COUNT = 6;

const MARKET_ORDER: Record<string, number> = {
  h2h: 0,
  spreads: 1,
  totals: 2,
  outrights: 3,
};

// ---- Helpers ----

function formatPrice(
  marketType: string,
  price: number | null,
  point: number | null,
): string {
  if (price === null) return "\u2014";
  if (marketType === "h2h" || marketType === "outrights") {
    return price >= 0 ? `+${Math.round(price)}` : `${Math.round(price)}`;
  }
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

function eventName(home: string, away: string): string {
  if (!away) return home || "Unknown Event";
  if (!home) return away;
  return `${away} @ ${home}`;
}

function marketLabel(type: string): string {
  switch (type) {
    case "h2h": return "Moneyline";
    case "spreads": return "Spread";
    case "totals": return "Total";
    case "outrights": return "Futures";
    default: return type;
  }
}

// ---- Components ----

function LinesTable({ rows }: { rows: EdgeComparisonRow[] }) {
  return (
    <div className="lines-table-wrap">
      <table className="table lines-table">
        <thead>
          <tr>
            <th className="table__th">Selection</th>
            <th className="table__th">Market</th>
            <th className="table__th table__th--right">Pinnacle</th>
            <th className="table__th table__th--right">DraftKings</th>
            <th className="table__th table__th--right">Edge</th>
            <th className="table__th">Signal</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr
              key={`${row.marketType}-${row.selectionKey}`}
              className={cn(rowVariant(row.signal) && `table__row--${rowVariant(row.signal)}`)}
            >
              <td className="table__td">{row.selectionKey}</td>
              <td className="table__td">
                <Badge variant="neutral">{marketLabel(row.marketType)}</Badge>
              </td>
              <td className="table__td table__td--right">
                {row.baselineBook
                  ? formatPrice(row.marketType, row.baselinePrice, row.baselinePoint)
                  : <span className="edge-missing">N/A</span>}
              </td>
              <td className="table__td table__td--right">
                {formatPrice(row.marketType, row.targetPrice, row.targetPoint)}
              </td>
              <td className="table__td table__td--right">
                <span className={cn(
                  "edge-pct",
                  row.edgePct !== null && row.edgePct >= 1 && "edge-pct--value",
                  row.edgePct !== null && row.edgePct <= -1 && "edge-pct--tax",
                )}>
                  {formatEdge(row.edgePct)}
                </span>
              </td>
              <td className="table__td">
                <Badge variant={signalVariant(row.signal)}>
                  {signalLabel(row.signal)}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ExpandedLinesRow({
  group,
  isOpen,
  id,
}: {
  group: EventGroup;
  isOpen: boolean;
  id: string;
}) {
  return (
    <tr id={id} className="expanded-lines-row">
      <td colSpan={OUTER_COL_COUNT} className="expanded-lines-cell">
        {isOpen && <LinesTable rows={group.rows} />}
      </td>
    </tr>
  );
}

function EventGroupRow({
  group,
  isOpen,
  onToggle,
  controlsId,
}: {
  group: EventGroup;
  isOpen: boolean;
  onToggle: () => void;
  controlsId: string;
}) {
  const variant = group.valueCount > 0
    ? "value"
    : group.taxCount > 0 ? "tax" : undefined;

  return (
    <tr
      className={cn(
        "event-group-header",
        variant && `table__row--${variant}`,
      )}
      onClick={() => {
        if (window.getSelection()?.toString()) return;
        onToggle();
      }}
    >
      <Td className="event-group-toggle">
        <button
          className="event-group-expander"
          aria-expanded={isOpen}
          aria-controls={controlsId}
          onClick={(e) => { e.stopPropagation(); onToggle(); }}
        >
          <span className={cn(
            "event-group-chevron",
            isOpen && "event-group-chevron--open",
          )}>
            &#9654;
          </span>
        </button>
      </Td>
      <Td className="event-group-event-cell">
        {eventName(group.homeTeam, group.awayTeam)}
      </Td>
      <Td className="event-group-meta-cell">
        {group.sportTitle}
      </Td>
      <Td className="event-group-meta-cell">
        {formatDateTime(group.commenceTimeUtc)}
      </Td>
      <Td align="right">
        {group.bestEdge !== null && (
          <span className={cn(
            "edge-pct",
            group.bestEdge >= 1 && "edge-pct--value",
            group.bestEdge <= -1 && "edge-pct--tax",
          )}>
            {formatEdge(group.bestEdge)}
          </span>
        )}
      </Td>
      <Td>
        <span className="event-group-signals">
          {group.valueCount > 0 && (
            <Badge variant="success">{group.valueCount}V</Badge>
          )}
          {group.taxCount > 0 && (
            <Badge variant="danger">{group.taxCount}T</Badge>
          )}
          <Badge variant="neutral">{group.rows.length}</Badge>
        </span>
      </Td>
    </tr>
  );
}

function EdgeScannerTable({
  groups,
  expanded,
  onToggle,
  sortCol,
  sortDir,
  onSort,
}: {
  groups: EventGroup[];
  expanded: Set<string>;
  onToggle: (eventId: string) => void;
  sortCol: SortColumn;
  sortDir: SortDir;
  onSort: (col: SortColumn) => void;
}) {
  function sortIndicator(col: SortColumn): string {
    if (sortCol !== col) return "";
    return sortDir === "asc" ? " \u25B2" : " \u25BC";
  }

  return (
    <Table>
      <thead>
        <tr>
          <Th style={{ width: "2rem" }}></Th>
          <Th
            className={cn("table__th--sortable", sortCol === "event" && "table__th--sorted")}
            onClick={() => onSort("event")}
          >
            Event{sortIndicator("event")}
          </Th>
          <Th>Competition</Th>
          <Th>Start Time</Th>
          <Th
            align="right"
            className={cn("table__th--sortable", sortCol === "edge" && "table__th--sorted")}
            onClick={() => onSort("edge")}
          >
            Edge{sortIndicator("edge")}
          </Th>
          <Th>Signals</Th>
        </tr>
      </thead>
      <tbody>
        {groups.map((group) => {
          const isOpen = expanded.has(group.eventId);
          const linesId = `lines-${group.eventId}`;
          return (
            <Fragment key={group.eventId}>
              <EventGroupRow
                group={group}
                isOpen={isOpen}
                onToggle={() => onToggle(group.eventId)}
                controlsId={linesId}
              />
              <ExpandedLinesRow
                group={group}
                isOpen={isOpen}
                id={linesId}
              />
            </Fragment>
          );
        })}
      </tbody>
    </Table>
  );
}

// ---- Page ----

export function DashboardPage() {
  const { data, isLoading, error } = useGetEdgeComparisonsQuery();
  const [sportGroup, setSportGroup] = useState("all");
  const [league, setLeague] = useState("all");
  const [signal, setSignal] = useState("all");
  const [search, setSearch] = useState("");
  const [sortCol, setSortCol] = useState<SortColumn>("edge");
  const [sortDir, setSortDir] = useState<SortDir>("desc");
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  function toggleSort(col: SortColumn) {
    if (sortCol === col) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortCol(col);
      setSortDir(col === "edge" ? "desc" : "asc");
    }
  }

  function toggleExpand(eventId: string) {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(eventId)) next.delete(eventId);
      else next.add(eventId);
      return next;
    });
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

  const filteredRows = useMemo(() => {
    if (!data) return [];
    const q = search.toLowerCase();
    return data.comparisons.filter((c) => {
      if (sportGroup !== "all" && c.sportGroup !== sportGroup) return false;
      if (league !== "all" && c.sportKey !== league) return false;
      if (signal !== "all" && c.signal !== signal) return false;
      if (q && !`${c.homeTeam} ${c.awayTeam}`.toLowerCase().includes(q)) return false;
      return true;
    });
  }, [data, sportGroup, league, signal, search]);

  const eventGroups = useMemo(() => {
    const map = new Map<string, EventGroup>();

    for (const row of filteredRows) {
      let group = map.get(row.eventId);
      if (!group) {
        group = {
          eventId: row.eventId,
          homeTeam: row.homeTeam,
          awayTeam: row.awayTeam,
          sportTitle: row.sportTitle,
          sportGroup: row.sportGroup,
          commenceTimeUtc: row.commenceTimeUtc,
          rows: [],
          bestEdge: null,
          valueCount: 0,
          taxCount: 0,
        };
        map.set(row.eventId, group);
      }
      group.rows.push(row);
      if (row.signal === "value") group.valueCount++;
      if (row.signal === "tax") group.taxCount++;
      if (row.edgePct !== null && (group.bestEdge === null || row.edgePct > group.bestEdge)) {
        group.bestEdge = row.edgePct;
      }
    }

    const groups = [...map.values()];

    for (const group of groups) {
      group.rows.sort((a, b) => {
        const aOrder = MARKET_ORDER[a.marketType] ?? 99;
        const bOrder = MARKET_ORDER[b.marketType] ?? 99;
        if (aOrder !== bOrder) return aOrder - bOrder;
        const aEdge = Math.abs(a.edgePct ?? 0);
        const bEdge = Math.abs(b.edgePct ?? 0);
        return bEdge - aEdge;
      });
    }

    const dir = sortDir === "asc" ? 1 : -1;
    groups.sort((a, b) => {
      if (sortCol === "event") {
        const aName = `${a.awayTeam} ${a.homeTeam}`.toLowerCase();
        const bName = `${b.awayTeam} ${b.homeTeam}`.toLowerCase();
        return aName < bName ? -dir : aName > bName ? dir : 0;
      }
      const aEdge = a.bestEdge ?? (dir > 0 ? Infinity : -Infinity);
      const bEdge = b.bestEdge ?? (dir > 0 ? Infinity : -Infinity);
      return (aEdge - bEdge) * dir;
    });

    return groups;
  }, [filteredRows, sortCol, sortDir]);

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
      </div>

      <div className="card">
        <div className="card__header">
          <span>Edge Scanner</span>
          <div className="row-end">
            {eventGroups.length > 0 && (
              <>
                <button
                  className="btn btn--ghost btn--sm"
                  onClick={() => setExpanded(new Set(eventGroups.map((g) => g.eventId)))}
                >
                  Expand All
                </button>
                <button
                  className="btn btn--ghost btn--sm"
                  onClick={() => setExpanded(new Set())}
                >
                  Collapse All
                </button>
              </>
            )}
            <Badge variant="neutral">
              {eventGroups.length} {eventGroups.length === 1 ? "event" : "events"}
            </Badge>
          </div>
        </div>
        {eventGroups.length === 0 ? (
          <div style={{ padding: "var(--space-4)" }}>
            <p className="placeholder">
              {data.comparisons.length === 0
                ? "No ingested odds data yet. Run an ingestion first."
                : "No edges match your filters."}
            </p>
          </div>
        ) : (
          <EdgeScannerTable
            groups={eventGroups}
            expanded={expanded}
            onToggle={toggleExpand}
            sortCol={sortCol}
            sortDir={sortDir}
            onSort={toggleSort}
          />
        )}
      </div>
    </div>
  );
}
