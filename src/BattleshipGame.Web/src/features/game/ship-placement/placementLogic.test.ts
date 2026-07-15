import { describe, expect, it } from 'vitest'
import { cellsForShip, fits, generateRandomPlacements } from './placementLogic'
import { SHIP_KINDS, SHIP_LENGTHS } from './shipLengths'

describe('cellsForShip', () => {
  it('lays a horizontal ship along increasing letters, same digit', () => {
    expect(cellsForShip('Cruiser', 'Horizontal', 'A1', 10)).toEqual(['A1', 'B1', 'C1'])
  })

  it('lays a vertical ship along increasing digits, same letter', () => {
    expect(cellsForShip('Cruiser', 'Vertical', 'A1', 10)).toEqual(['A1', 'A2', 'A3'])
  })

  it('returns the ship length worth of cells', () => {
    expect(cellsForShip('Carrier', 'Horizontal', 'A1', 10)).toHaveLength(SHIP_LENGTHS.Carrier)
  })

  it('returns null when a horizontal ship runs off the right edge', () => {
    // Carrier (5) starting at H1 on a 10-wide board would need H,I,J,K,L.
    expect(cellsForShip('Carrier', 'Horizontal', 'H1', 10)).toBeNull()
  })

  it('returns null when a vertical ship runs off the bottom edge', () => {
    expect(cellsForShip('Carrier', 'Vertical', 'A8', 10)).toBeNull()
  })

  it('allows a ship that ends exactly on the last cell', () => {
    expect(cellsForShip('Destroyer', 'Horizontal', 'I1', 10)).toEqual(['I1', 'J1'])
    expect(cellsForShip('Destroyer', 'Vertical', 'A9', 10)).toEqual(['A9', 'A10'])
  })
})

describe('fits', () => {
  it('is true when no candidate cell is occupied', () => {
    expect(fits(['A1', 'B1'], new Set(['C1', 'D1']))).toBe(true)
  })

  it('is false when any candidate cell is occupied', () => {
    expect(fits(['A1', 'B1'], new Set(['B1']))).toBe(false)
  })
})

describe('generateRandomPlacements', () => {
  it('places all five ships without overlap and within bounds', () => {
    const placements = generateRandomPlacements(10)
    expect(placements).toHaveLength(SHIP_KINDS.length)

    const occupied = new Set<string>()
    for (const p of placements) {
      const cells = cellsForShip(p.shipKind, p.orientation, p.bowCode, 10)
      expect(cells).not.toBeNull()
      expect(fits(cells!, occupied)).toBe(true)
      cells!.forEach((c) => occupied.add(c))
    }

    const totalCells = Object.values(SHIP_LENGTHS).reduce((a, b) => a + b, 0)
    expect(occupied.size).toBe(totalCells)
  })

  it('is deterministic for a fixed random source', () => {
    const seq = [0.1, 0.2, 0.3, 0.4, 0.5]
    let i = 0
    const random = () => seq[i++ % seq.length]
    const a = generateRandomPlacements(10, random)
    i = 0
    const b = generateRandomPlacements(10, random)
    expect(a).toEqual(b)
  })
})
