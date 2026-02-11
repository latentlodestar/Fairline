import { Navigate, NavLink, Route, Routes } from "react-router-dom";
import { ThemeToggle } from "./components/ThemeToggle";
import { DashboardPage } from "./pages/DashboardPage";
import { IngestionPage } from "./pages/IngestionPage";
import { StatusPage } from "./pages/StatusPage";
import { StyleGuidePage } from "./pages/StyleGuidePage";
import { cn } from "./lib/cn";

const navItems = [
  { to: "/dashboard", label: "Dashboard" },
  { to: "/ingestion", label: "Ingestion" },
  { to: "/status", label: "Status" },
  { to: "/style-guide", label: "Style Guide" },
] as const;

export default function App() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="topbar__brand">Fairline</div>
        <nav className="topbar__nav">
          {navItems.map(({ to, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn("topbar__link", isActive && "topbar__link--active")
              }
            >
              {label}
            </NavLink>
          ))}
        </nav>
        <ThemeToggle />
      </header>
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/ingestion" element={<IngestionPage />} />
          <Route path="/status" element={<StatusPage />} />
          <Route path="/style-guide" element={<StyleGuidePage />} />
        </Routes>
      </main>
    </div>
  );
}
