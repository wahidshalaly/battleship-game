import { describe, expect, it } from 'vitest'
import type { components } from '../../../api/schema'
import { applyRound, emptyBattleState } from './roundHistory'

type LastRoundResult = components['schemas']['LastRoundResult']

// The API returns null for cells/results when a round has no counter-attack, even though
// the generated schema types them as non-nullable — so the helper accepts a loose shape.
function round(partial: Record<string, unknown>): LastRoundResult {
  return { gameState: 'Started', winnerSide: 'None', ...partial } as LastRoundResult
}

describe('applyRound', () => {
  it('records the player shot on the opponent board and the incoming shot on the own board', () => {
    const next = applyRound(
      emptyBattleState,
      round({
        playerTargetCell: 'A6',
        playerAttackResult: 'Hit',
        opponentTargetCell: 'G2',
        opponentAttackResult: 'Missed',
      }),
    )
    expect(next.myShots).toEqual({ A6: 'Hit' })
    expect(next.incomingShots).toEqual({ G2: 'Missed' })
  })

  it('accumulates across rounds without mutating prior state', () => {
    const first = applyRound(
      emptyBattleState,
      round({ playerTargetCell: 'A1', playerAttackResult: 'Missed' }),
    )
    const second = applyRound(first, round({ playerTargetCell: 'B1', playerAttackResult: 'Hit' }))
    expect(second.myShots).toEqual({ A1: 'Missed', B1: 'Hit' })
    expect(first.myShots).toEqual({ A1: 'Missed' }) // unchanged
  })

  it('ignores a missing counter-attack (game ended on the player attack)', () => {
    const next = applyRound(
      emptyBattleState,
      round({
        playerTargetCell: 'B10',
        playerAttackResult: 'Hit',
        opponentTargetCell: null,
        opponentAttackResult: null,
        gameState: 'GameOver',
        winnerSide: 'Player',
      }),
    )
    expect(next.myShots).toEqual({ B10: 'Hit' })
    expect(next.incomingShots).toEqual({})
  })
})
