import { cn } from "../lib/cn";
import type { ReactNode } from "react";

interface BadgeProps {
  variant?: "neutral" | "success" | "danger" | "warning";
  children: ReactNode;
  className?: string;
}

export function Badge({
  variant = "neutral",
  children,
  className,
}: BadgeProps) {
  return (
    <span className={cn("badge", `badge--${variant}`, className)}>
      {children}
    </span>
  );
}
