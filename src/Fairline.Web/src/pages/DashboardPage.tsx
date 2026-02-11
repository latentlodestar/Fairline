import { useState } from "react";
import { KpiCard } from "../components/KpiCard";
import { Badge } from "../components/Badge";
import { Table, Th, Td, Tr } from "../components/Table";
import { Select, Input } from "../components/Input";

const PLACEHOLDER_EDGES = [
  { id: 1, event: "LAL vs BOS", market: "Spread", book: "FanDuel", fairLine: "-3.5", bookLine: "-4.5", edge: "+2.8%", type: "value" as const },
  { id: 2, event: "LAL vs BOS", market: "Total", book: "DraftKings", fairLine: "218.5", bookLine: "217", edge: "-1.4%", type: "tax" as const },
  { id: 3, event: "NYK vs MIA", market: "ML", book: "BetMGM", fairLine: "-145", bookLine: "-155", edge: "-3.2%", type: "tax" as const },
  { id: 4, event: "NYK vs MIA", market: "Spread", book: "Caesars", fairLine: "-2.5", bookLine: "-2.5", edge: "+0.1%", type: "neutral" as const },
  { id: 5, event: "GSW vs DEN", market: "ML", book: "FanDuel", fairLine: "+180", bookLine: "+195", edge: "+4.1%", type: "value" as const },
  { id: 6, event: "GSW vs DEN", market: "Total", book: "DraftKings", fairLine: "226", bookLine: "224.5", edge: "-0.8%", type: "neutral" as const },
  { id: 7, event: "PHI vs MIL", market: "Spread", book: "BetMGM", fairLine: "+1.5", bookLine: "+1", edge: "-2.1%", type: "tax" as const },
  { id: 8, event: "PHI vs MIL", market: "ML", book: "Caesars", fairLine: "+115", bookLine: "+125", edge: "+3.5%", type: "value" as const },
];

export function DashboardPage() {
  const [sport, setSport] = useState("all");
  const [search, setSearch] = useState("");

  const filtered = PLACEHOLDER_EDGES.filter(
    (e) =>
      (sport === "all" || true) &&
      (search === "" ||
        e.event.toLowerCase().includes(search.toLowerCase())),
  );

  return (
    <div className="dashboard">
      <div className="kpi-strip">
        <KpiCard label="Markets Tracked" value="1,247" />
        <KpiCard
          label="Value Edges"
          value="83"
          trend="up"
          delta="+12 today"
        />
        <KpiCard
          label="Tax Alerts"
          value="214"
          trend="down"
          delta="-8 today"
        />
        <KpiCard
          label="Avg. Edge"
          value="2.4%"
          trend="neutral"
          delta="\u00B10.1%"
        />
      </div>

      <div className="filter-bar">
        <Select value={sport} onChange={(e) => setSport(e.target.value)}>
          <option value="all">All Sports</option>
          <option value="nba">NBA</option>
          <option value="nfl">NFL</option>
          <option value="mlb">MLB</option>
          <option value="nhl">NHL</option>
        </Select>
        <Select>
          <option value="all">All Markets</option>
          <option value="spread">Spread</option>
          <option value="ml">Moneyline</option>
          <option value="total">Total</option>
        </Select>
        <Select>
          <option value="all">All Books</option>
          <option value="fanduel">FanDuel</option>
          <option value="draftkings">DraftKings</option>
          <option value="betmgm">BetMGM</option>
          <option value="caesars">Caesars</option>
        </Select>
        <Input
          type="search"
          placeholder="Search events 2026"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="card">
        <div className="card__header">
          <span>Edge Scanner</span>
          <Badge variant="neutral">{filtered.length} results</Badge>
        </div>
        <Table>
          <thead>
            <tr>
              <Th>Event</Th>
              <Th>Market</Th>
              <Th>Book</Th>
              <Th align="right">Fair Line</Th>
              <Th align="right">Book Line</Th>
              <Th align="right">Edge</Th>
              <Th>Signal</Th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((row) => (
              <Tr
                key={row.id}
                variant={
                  row.type === "value"
                    ? "value"
                    : row.type === "tax"
                      ? "tax"
                      : undefined
                }
              >
                <Td>{row.event}</Td>
                <Td>{row.market}</Td>
                <Td>{row.book}</Td>
                <Td align="right">{row.fairLine}</Td>
                <Td align="right">{row.bookLine}</Td>
                <Td align="right">{row.edge}</Td>
                <Td>
                  <Badge
                    variant={
                      row.type === "value"
                        ? "success"
                        : row.type === "tax"
                          ? "danger"
                          : "neutral"
                    }
                  >
                    {row.type === "value"
                      ? "Value"
                      : row.type === "tax"
                        ? "Tax"
                        : "Fair"}
                  </Badge>
                </Td>
              </Tr>
            ))}
          </tbody>
        </Table>
      </div>
    </div>
  );
}
