import type { ReactNode } from "react";
import "./StatusCard.css";

interface StatusCardProps {
  title: string;
  children: ReactNode;
  status?: "ok" | "error" | "loading";
}

export function StatusCard({ title, children, status = "ok" }: StatusCardProps) {
  return (
    <div className={`status-card status-card--${status}`}>
      <h3 className="status-card__title">{title}</h3>
      <div className="status-card__body">{children}</div>
    </div>
  );
}
