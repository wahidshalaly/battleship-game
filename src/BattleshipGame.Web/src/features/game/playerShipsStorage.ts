// The API doesn't return a player's own ship layout, so the battle screen can't rebuild
// it from the server. We stash the placed cells in sessionStorage during placement and
// read them back in battle. If they're missing (e.g. a resumed game in a new tab), the
// own board simply shows incoming hits/misses without the ship outline.
const key = (gameId: string) => `bg_player_ships_${gameId}`

export function savePlayerShips(gameId: string, cells: string[]): void {
  sessionStorage.setItem(key(gameId), JSON.stringify(cells))
}

export function loadPlayerShips(gameId: string): string[] {
  const raw = sessionStorage.getItem(key(gameId))
  if (!raw) return []
  try {
    return JSON.parse(raw) as string[]
  } catch {
    return []
  }
}
