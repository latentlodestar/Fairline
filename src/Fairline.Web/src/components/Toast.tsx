import { cn } from "../lib/cn";

interface ToastProps {
  message: string;
  variant?: "success" | "danger" | "warning" | "neutral";
  onDismiss?: () => void;
}

export function Toast({
  message,
  variant = "neutral",
  onDismiss,
}: ToastProps) {
  return (
    <div className={cn("toast", `toast--${variant}`)}>
      <span className="toast__message">{message}</span>
      {onDismiss && (
        <button
          className="toast__dismiss"
          onClick={onDismiss}
          aria-label="Dismiss"
        >
          &times;
        </button>
      )}
    </div>
  );
}
