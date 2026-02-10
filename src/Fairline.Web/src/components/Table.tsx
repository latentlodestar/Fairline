import { cn } from "../lib/cn";
import type {
  ReactNode,
  ThHTMLAttributes,
  TdHTMLAttributes,
  HTMLAttributes,
} from "react";

interface TableProps extends HTMLAttributes<HTMLTableElement> {
  children: ReactNode;
}

export function Table({ className, children, ...props }: TableProps) {
  return (
    <div className="table-wrap">
      <table className={cn("table", className)} {...props}>
        {children}
      </table>
    </div>
  );
}

interface ThProps extends ThHTMLAttributes<HTMLTableCellElement> {
  align?: "left" | "right";
}

export function Th({ className, align, children, ...props }: ThProps) {
  return (
    <th
      className={cn(
        "table__th",
        align === "right" && "table__th--right",
        className,
      )}
      {...props}
    >
      {children}
    </th>
  );
}

interface TdProps extends TdHTMLAttributes<HTMLTableCellElement> {
  align?: "left" | "right";
}

export function Td({ className, align, children, ...props }: TdProps) {
  return (
    <td
      className={cn(
        "table__td",
        align === "right" && "table__td--right",
        className,
      )}
      {...props}
    >
      {children}
    </td>
  );
}

interface TrProps extends HTMLAttributes<HTMLTableRowElement> {
  variant?: "value" | "tax";
  children: ReactNode;
}

export function Tr({ variant, className, children, ...props }: TrProps) {
  return (
    <tr
      className={cn(variant && `table__row--${variant}`, className)}
      {...props}
    >
      {children}
    </tr>
  );
}
