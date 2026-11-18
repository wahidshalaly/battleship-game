import { useMemo, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { problemMessage } from '../../../api/problemDetails'
import type { components } from '../../../api/schema'
import { BoardGrid } from '../../../components/BoardGrid'
import { useAttack, useGameBoards } from '../gameQueries'
import { toBattleState, type BoardState } from './battleState'

type ShipKind = components['schemas']['ShipKind']

interface BattleScreenProps {
  gameId: string
  boardSize: number
}

function sunkMessage(playerSunk?: ShipKind, opponentSunk?: ShipKind): string | null {
  const parts: string[] = []
  if (playerSunk && playerSunk !== 'None') parts.push(`You sank the opponent's ${playerSunk}!`)
  if (opponentSunk && opponentSunk !== 'None') parts.push(`Your ${opponentSunk} was sunk!`)
  return parts.length > 0 ? parts.join(' ') : null
}

function cellClass(board: BoardState, code: string): string | null {
  if (board.sunkCells.has(code)) return 'bg-red-900 border-red-700'
  const shot = board.shots[code]
  if (shot === 'Hit') return 'bg-red-600 border-red-400'
  if (shot === 'Missed') return 'bg-slate-600 border-slate-500'
  if (board.shipCells.has(code)) return 'bg-cyan-700 border-cyan-500'
  return null
}

export function BattleScreen({ gameId, boardSize }: BattleScreenProps) {
  const queryClient = useQueryClient()
  const boards = useGameBoards(gameId)
  const attack = useAttack(gameId)
  const battle = useMemo(() => toBattleState(boards.data), [boards.data])

  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [gameOver, setGameOver] = useState(false)

  async function handleAttack(cell: string) {
    if (gameOver || attack.isPending || cell in battle.opponent.shots) return
    setError(null)
    setMessage(null)
    try {
      const round = await attack.mutateAsync(cell)
      setMessage(sunkMessage(round.playerSunkShip, round.opponentSunkShip))
      if (round.gameState === 'GameOver') {
        setGameOver(true)
        // Refresh the game so GamePage swaps to the result view.
        void queryClient.invalidateQueries({ queryKey: ['game', gameId] })
        void queryClient.invalidateQueries({ queryKey: ['game', 'active'] })
      }
    } catch (err) {
      setError(problemMessage(err, 'That attack was rejected.'))
    }
  }

  function opponentCellClass(code: string): string {
    return (
      cellClass(battle.opponent, code) ??
      (gameOver ? 'bg-slate-800' : 'bg-slate-800 hover:bg-cyan-800 cursor-crosshair')
    )
  }

  function myCellClass(code: string): string {
    return cellClass(battle.own, code) ?? 'bg-slate-800'
  }

  if (boards.isLoading) {
    return <p className="text-slate-400">Loading boards…</p>
  }

  if (boards.error) {
    return (
      <p role="alert" className="text-red-400">
        {problemMessage(boards.error, 'Could not load the boards.')}
      </p>
    )
  }

  return (
    <div>
      <h2 className="mb-1 text-xl font-semibold">Battle</h2>
      <p className="mb-6 h-5 text-sm text-cyan-300">{message}</p>

      <div className="flex flex-col gap-10 lg:flex-row">
        <BoardGrid
          boardSize={boardSize}
          label="Opponent — click to attack"
          cellClassName={opponentCellClass}
          onCellClick={(code) => void handleAttack(code)}
          disabled={gameOver || attack.isPending}
        />
        <BoardGrid boardSize={boardSize} label="Your fleet" cellClassName={myCellClass} disabled />
      </div>

      {error && (
        <p role="alert" className="mt-4 text-sm text-red-400">
          {error}
        </p>
      )}
    </div>
  )
}
