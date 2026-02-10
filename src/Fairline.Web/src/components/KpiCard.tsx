import { cn } from "../lib/cn";

interface KpiCardProps {
  label: string;
  value: string | number;
  delta?: string;
  trend?: "up" | "down" | "neutral";
  className?: string;
}

export function KpiCard({
  label,
  value,
  delta,
  trend,
  className,
}: KpiCardProps) {
  return (
    <div className={cn("kpi", className)}>
      <div className="kpi__label">{label}</div>
      <div className="kpi__value">{value}</div>
      {delta && (
        <div className={cn("kpi__delta", trend && `kpi__delta--${trend}`)}>
          {delta}
        </div>
      )}
    </div>
  );
}
