import { describe, expect, it } from 'vitest'
import type { components } from '../../../api/schema'
import { applyRound, toBattleState, type GameBoards } from './battleState'

type LastRoundResult = components['schemas']['LastRoundResult']

// The API returns null for cells/results when a round has no counter-attack, even though
// the generated schema types them as non-nullable — so the helper accepts a loose shape.
function round(partial: Record<string, unknown>): LastRoundResult {
  return { gameState: 'Started', winnerSide: 'None', ...partial } as LastRoundResult
}

const emptyBoards: GameBoards = {
  gameId: 'g1',
  ownBoard: { ships: [], hits: [], misses: [] },
  opponentBoard: { ships: [], hits: [], misses: [] },
}

describe('applyRound', () => {
  it('records the player shot on the opponent board and the incoming shot on the own board', () => {
    const next = applyRound(
      emptyBoards,
      round({
        playerTargetCell: 'A6',
        playerAttackResult: 'Hit',
        opponentTargetCell: 'G2',
        opponentAttackResult: 'Missed',
      }),
    )
    expect(next.opponentBoard).toMatchObject({ hits: ['A6'], misses: [] })
    expect(next.ownBoard).toMatchObject({ hits: [], misses: ['G2'] })
  })

  it('adds to the shots already on the boards without mutating them', () => {
    const boards: GameBoards = {
      ...emptyBoards,
      opponentBoard: { ships: [], hits: [], misses: ['A1'] },
    }
    const next = applyRound(boards, round({ playerTargetCell: 'B1', playerAttackResult: 'Hit' }))
    expect(next.opponentBoard).toMatchObject({ hits: ['B1'], misses: ['A1'] })
    expect(boards.opponentBoard).toEqual({ ships: [], hits: [], misses: ['A1'] })
  })

  it('does not record the same shot twice', () => {
    const boards: GameBoards = {
      ...emptyBoards,
      opponentBoard: { ships: [], hits: ['B1'], misses: [] },
    }
    const next = applyRound(boards, round({ playerTargetCell: 'B1', playerAttackResult: 'Hit' }))
    expect(next.opponentBoard?.hits).toEqual(['B1'])
  })

  it('ignores a missing counter-attack (game ended on the player attack)', () => {
    const next = applyRound(
      emptyBoards,
      round({
        playerTargetCell: 'B10',
        playerAttackResult: 'Hit',
        opponentTargetCell: null,
        opponentAttackResult: null,
        gameState: 'GameOver',
        winnerSide: 'Player',
      }),
    )
    expect(next.opponentBoard?.hits).toEqual(['B10'])
    expect(next.ownBoard).toEqual(emptyBoards.ownBoard)
  })
})

describe('toBattleState', () => {
  it('maps the shots on each board to their result', () => {
    const state = toBattleState({
      ...emptyBoards,
      ownBoard: { ships: [], hits: ['C3'], misses: ['D4'] },
      opponentBoard: { ships: [], hits: ['A1'], misses: ['B2'] },
    })
    expect(state.own.shots).toEqual({ C3: 'Hit', D4: 'Missed' })
    expect(state.opponent.shots).toEqual({ A1: 'Hit', B2: 'Missed' })
  })

  it('lists the cells of every visible ship, and of the sunk ones separately', () => {
    const state = toBattleState({
      ...emptyBoards,
      ownBoard: {
        ships: [
          { kind: 'Destroyer', position: ['A1', 'A2'], sunk: true },
          { kind: 'Cruiser', position: ['C1', 'C2', 'C3'], sunk: false },
        ],
        hits: ['A1', 'A2'],
        misses: [],
      },
      opponentBoard: {
        ships: [{ kind: 'Submarine', position: ['E5', 'F5', 'G5'], sunk: true }],
        hits: ['E5', 'F5', 'G5'],
        misses: [],
      },
    })
    expect([...state.own.shipCells]).toEqual(['A1', 'A2', 'C1', 'C2', 'C3'])
    expect([...state.own.sunkCells]).toEqual(['A1', 'A2'])
    expect([...state.opponent.shipCells]).toEqual(['E5', 'F5', 'G5'])
    expect([...state.opponent.sunkCells]).toEqual(['E5', 'F5', 'G5'])
  })

  it('treats missing boards as empty', () => {
    const state = toBattleState(undefined)
    expect(state.own).toEqual({ shots: {}, shipCells: new Set(), sunkCells: new Set() })
    expect(state.opponent).toEqual({ shots: {}, shipCells: new Set(), sunkCells: new Set() })
  })
})
