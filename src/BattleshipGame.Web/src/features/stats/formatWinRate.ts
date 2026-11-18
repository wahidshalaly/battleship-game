/** Formats a win rate from 0 to 1 as a whole percentage, e.g. 0.667 → "67%". */
export function formatWinRate(winRate: number | undefined): string {
  return `${Math.round((winRate ?? 0) * 100)}%`
}
