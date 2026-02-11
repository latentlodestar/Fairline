export function formatDateTime(iso: string): string {
  const d = new Date(iso);
  return `${d.toLocaleDateString("en-US")} @ ${d.toLocaleTimeString("en-US")}`;
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString("en-US");
}
