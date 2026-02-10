/**
 * Compose class names, filtering out falsy values.
 *
 * Usage:
 *   cn("btn", variant && `btn--${variant}`, className)
 */
export function cn(
  ...classes: (string | false | null | undefined)[]
): string {
  return classes.filter(Boolean).join(" ");
}
