import type { components } from '../../../api/schema'
import { SHIP_KINDS, SHIP_LENGTHS, type PlaceableShipKind } from './shipLengths'

type ShipOrientation = components['schemas']['ShipOrientation']

export interface ShipPlacement {
  shipKind: PlaceableShipKind
  orientation: ShipOrientation
  bowCode: string
}

const LETTERS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'

export interface CellCoord {
  letter: string
  digit: number
}

export function parseCell(code: string): CellCoord {
  return { letter: code[0], digit: Number(code.slice(1)) }
}

export function toCode(letter: string, digit: number): string {
  return `${letter}${digit}`
}

/**
 * Returns the cell codes a ship occupies, or null if it would run off the board.
 * Matches the server's convention (ShipPositionCalculator): Vertical increases the
 * digit (down a column), Horizontal increases the letter (across a row).
 */
export function cellsForShip(
  kind: PlaceableShipKind,
  orientation: ShipOrientation,
  bowCode: string,
  boardSize: number,
): string[] | null {
  const length = SHIP_LENGTHS[kind]
  const { letter, digit } = parseCell(bowCode)
  const startColumn = LETTERS.indexOf(letter)
  const cells: string[] = []

  for (let i = 0; i < length; i++) {
    if (orientation === 'Vertical') {
      const d = digit + i
      if (d > boardSize) return null
      cells.push(toCode(letter, d))
    } else {
      const c = startColumn + i
      if (c >= boardSize) return null
      cells.push(toCode(LETTERS[c], digit))
    }
  }

  return cells
}

/** True if the candidate cells don't collide with any already-occupied cell. */
export function fits(candidate: string[], occupied: ReadonlySet<string>): boolean {
  return candidate.every((cell) => !occupied.has(cell))
}

/**
 * Generates a random, non-overlapping placement for all five ships. Used to set up the
 * opponent's board — the API requires the client to place both boards, so the frontend
 * places the opponent's ships at random positions the player never sees.
 */
export function generateRandomPlacements(
  boardSize: number,
  random: () => number = Math.random,
): ShipPlacement[] {
  const orientations: ShipOrientation[] = ['Horizontal', 'Vertical']
  const letters = LETTERS.slice(0, boardSize)
  const occupied = new Set<string>()
  const placements: ShipPlacement[] = []

  for (const shipKind of SHIP_KINDS) {
    // Bounded retry: a board this sparse is placed well within the attempt limit.
    for (let attempt = 0; attempt < 1000; attempt++) {
      const orientation = orientations[Math.floor(random() * orientations.length)]
      const letter = letters[Math.floor(random() * boardSize)]
      const digit = Math.floor(random() * boardSize) + 1
      const bowCode = toCode(letter, digit)
      const cells = cellsForShip(shipKind, orientation, bowCode, boardSize)
      if (cells && fits(cells, occupied)) {
        cells.forEach((c) => occupied.add(c))
        placements.push({ shipKind, orientation, bowCode })
        break
      }
    }
  }

  if (placements.length !== SHIP_KINDS.length) {
    throw new Error('Failed to generate random ship placements')
  }
  return placements
}
