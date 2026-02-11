type LogLevel = "debug" | "info" | "warn" | "error";

interface LogEntry {
  source: string;
  level: LogLevel;
  message: string;
  data?: string;
}

const FLUSH_INTERVAL_MS = 2000;
const FLUSH_THRESHOLD = 20;
const isDev = import.meta.env.DEV;

let buffer: LogEntry[] = [];
let timer: ReturnType<typeof setTimeout> | null = null;

function flush() {
  if (buffer.length === 0) return;
  const entries = buffer;
  buffer = [];
  if (timer) {
    clearTimeout(timer);
    timer = null;
  }

  fetch("/api/log", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(entries),
  }).catch(() => {
    // Silently drop — avoid recursive logging
  });
}

function enqueue(entry: LogEntry) {
  buffer.push(entry);
  if (buffer.length >= FLUSH_THRESHOLD) {
    flush();
  } else if (!timer) {
    timer = setTimeout(flush, FLUSH_INTERVAL_MS);
  }
}

if (typeof window !== "undefined") {
  window.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "hidden") {
      if (buffer.length > 0) {
        const entries = buffer;
        buffer = [];
        navigator.sendBeacon(
          "/api/log",
          new Blob([JSON.stringify(entries)], {
            type: "application/json",
          }),
        );
      }
    }
  });

  window.addEventListener("pagehide", () => {
    if (buffer.length > 0) {
      const entries = buffer;
      buffer = [];
      navigator.sendBeacon(
        "/api/log",
        new Blob([JSON.stringify(entries)], { type: "application/json" }),
      );
    }
  });
}

const shouldSend = (level: LogLevel) =>
  isDev || level === "warn" || level === "error";

export function createLogger(source: string) {
  const log = (level: LogLevel, message: string, data?: unknown) => {
    const serializedData = data !== undefined ? JSON.stringify(data) : undefined;

    if (isDev) {
      const consoleFn =
        level === "debug"
          ? console.debug
          : level === "info"
            ? console.info
            : level === "warn"
              ? console.warn
              : console.error;
      consoleFn(`[${source}]`, message, data ?? "");
    }

    if (shouldSend(level)) {
      enqueue({ source, level, message, data: serializedData });
    }
  };

  return {
    debug: (message: string, data?: unknown) => log("debug", message, data),
    info: (message: string, data?: unknown) => log("info", message, data),
    warn: (message: string, data?: unknown) => log("warn", message, data),
    error: (message: string, data?: unknown) => log("error", message, data),
  };
}
