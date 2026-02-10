import { useState } from "react";
import { ThemeToggle } from "./components/ThemeToggle";
import { DashboardPage } from "./pages/DashboardPage";
import { StatusPage } from "./pages/StatusPage";
import { StyleGuidePage } from "./pages/StyleGuidePage";
import { cn } from "./lib/cn";

type Page = "dashboard" | "status" | "styleguide";

export default function App() {
  const [page, setPage] = useState<Page>("dashboard");

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="topbar__brand">Fairline</div>
        <nav className="topbar__nav">
          {(
            [
              ["dashboard", "Dashboard"],
              ["status", "Status"],
              ["styleguide", "Style Guide"],
            ] as const
          ).map(([key, label]) => (
            <button
              key={key}
              className={cn(
                "topbar__link",
                page === key && "topbar__link--active",
              )}
              onClick={() => setPage(key)}
            >
              {label}
            </button>
          ))}
        </nav>
        <ThemeToggle />
      </header>
      <main className="main-content">
        {page === "dashboard" && <DashboardPage />}
        {page === "status" && <StatusPage />}
        {page === "styleguide" && <StyleGuidePage />}
      </main>
    </div>
  );
}
