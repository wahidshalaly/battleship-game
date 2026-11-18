import type { components } from '../../../api/schema'

type LastRoundResult = components['schemas']['LastRoundResult']
type CellState = components['schemas']['CellState']
type BoardView = components['schemas']['BoardView']

export type GameBoards = components['schemas']['GetGameBoardsQueryResult']

export type ShotResult = Extract<CellState, 'Hit' | 'Missed'>

/** What the battle screen draws on one board, keyed by cell code for quick lookup. */
export interface BoardState {
  // Cells attacked on this board, and the result.
  shots: Record<string, ShotResult>
  // Cells of the ships visible on this board: all own ships, only sunk opponent ships.
  shipCells: ReadonlySet<string>
  sunkCells: ReadonlySet<string>
}

export interface BattleState {
  own: BoardState
  opponent: BoardState
}

function isShotResult(value: CellState | null | undefined): value is ShotResult {
  return value === 'Hit' || value === 'Missed'
}

function withShot(
  board: BoardView | undefined,
  cell: string | null | undefined,
  result: CellState | null | undefined,
): BoardView | undefined {
  if (!cell || !isShotResult(result)) return board
  const key = result === 'Hit' ? 'hits' : 'misses'
  const cells = board?.[key] ?? []
  if (cells.includes(cell)) return board
  return { ...board, [key]: [...cells, cell] }
}

/**
 * Folds one round (a single POST /attacks response, which carries both the player's and
 * the opponent's outcomes) into the boards. Pure — returns new boards.
 */
export function applyRound(boards: GameBoards, round: LastRoundResult): GameBoards {
  return {
    ...boards,
    opponentBoard: withShot(boards.opponentBoard, round.playerTargetCell, round.playerAttackResult),
    ownBoard: withShot(boards.ownBoard, round.opponentTargetCell, round.opponentAttackResult),
  }
}

function toBoardState(board: BoardView | undefined): BoardState {
  const shots: Record<string, ShotResult> = {}
  for (const cell of board?.hits ?? []) shots[cell] = 'Hit'
  for (const cell of board?.misses ?? []) shots[cell] = 'Missed'

  const ships = board?.ships ?? []
  const shipCells = new Set(ships.flatMap((ship) => ship.position ?? []))
  const sunkCells = new Set(
    ships.filter((ship) => ship.sunk).flatMap((ship) => ship.position ?? []),
  )

  return { shots, shipCells, sunkCells }
}

/** Derives what the battle screen draws from the boards the API returns. */
export function toBattleState(boards: GameBoards | undefined): BattleState {
  return { own: toBoardState(boards?.ownBoard), opponent: toBoardState(boards?.opponentBoard) }
}
