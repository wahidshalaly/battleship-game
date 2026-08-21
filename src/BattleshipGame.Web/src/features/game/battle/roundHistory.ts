import type { components } from '../../../api/schema'

type LastRoundResult = components['schemas']['LastRoundResult']
type CellState = components['schemas']['CellState']

export type ShotResult = Extract<CellState, 'Hit' | 'Missed'>

export interface BattleState {
  // Cells the player has attacked on the opponent's board, and the result.
  myShots: Record<string, ShotResult>
  // Cells the opponent has attacked on the player's board, and the result.
  incomingShots: Record<string, ShotResult>
}

export const emptyBattleState: BattleState = { myShots: {}, incomingShots: {} }

function isShotResult(value: CellState | null | undefined): value is ShotResult {
  return value === 'Hit' || value === 'Missed'
}

/**
 * Folds one round (a single POST /attacks response, which carries both the player's and
 * the opponent's outcomes) into the accumulated board view. Pure — returns a new state.
 */
export function applyRound(state: BattleState, round: LastRoundResult): BattleState {
  const myShots = { ...state.myShots }
  const incomingShots = { ...state.incomingShots }

  if (round.playerTargetCell && isShotResult(round.playerAttackResult)) {
    myShots[round.playerTargetCell] = round.playerAttackResult
  }
  if (round.opponentTargetCell && isShotResult(round.opponentAttackResult)) {
    incomingShots[round.opponentTargetCell] = round.opponentAttackResult
  }

  return { myShots, incomingShots }
}
