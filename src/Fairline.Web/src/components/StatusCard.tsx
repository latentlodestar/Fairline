import { cn } from "../lib/cn";
import type { ReactNode } from "react";

interface StatusCardProps {
  title: string;
  children: ReactNode;
  status?: "ok" | "error" | "loading";
}

export function StatusCard({
  title,
  children,
  status = "ok",
}: StatusCardProps) {
  return (
    <div
      className={cn(
        "card",
        status === "ok" && "card--ok",
        status === "error" && "card--error",
        status === "loading" && "card--warning",
      )}
    >
      <div className="card__header">{title}</div>
      <div className="card__body">{children}</div>
    </div>
  );
}
