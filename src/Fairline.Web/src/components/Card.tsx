import { cn } from "../lib/cn";
import type { ReactNode } from "react";

interface CardProps {
  className?: string;
  children: ReactNode;
  status?: "ok" | "error" | "warning";
}

export function Card({ className, children, status }: CardProps) {
  return (
    <div
      className={cn(
        "card",
        status === "ok" && "card--ok",
        status === "error" && "card--error",
        status === "warning" && "card--warning",
        className,
      )}
    >
      {children}
    </div>
  );
}

export function CardHeader({
  className,
  children,
}: {
  className?: string;
  children: ReactNode;
}) {
  return <div className={cn("card__header", className)}>{children}</div>;
}

export function CardBody({
  className,
  children,
}: {
  className?: string;
  children: ReactNode;
}) {
  return <div className={cn("card__body", className)}>{children}</div>;
}

export function CardFooter({
  className,
  children,
}: {
  className?: string;
  children: ReactNode;
}) {
  return <div className={cn("card__footer", className)}>{children}</div>;
}
