import { useMemo, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { problemMessage } from '../../../api/problemDetails'
import type { components } from '../../../api/schema'
import { BoardGrid } from '../../../components/BoardGrid'
import { useAttack } from '../gameQueries'
import { loadPlayerShips } from '../playerShipsStorage'
import { applyRound, emptyBattleState } from './roundHistory'

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

export function BattleScreen({ gameId, boardSize }: BattleScreenProps) {
  const queryClient = useQueryClient()
  const attack = useAttack(gameId)
  const playerShips = useMemo(() => new Set(loadPlayerShips(gameId)), [gameId])

  const [battle, setBattle] = useState(emptyBattleState)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [gameOver, setGameOver] = useState(false)

  async function handleAttack(cell: string) {
    if (gameOver || attack.isPending || cell in battle.myShots) return
    setError(null)
    setMessage(null)
    try {
      const round = await attack.mutateAsync(cell)
      setBattle((prev) => applyRound(prev, round))
      setMessage(sunkMessage(round.playerSunkShip, round.opponentSunkShip))
      if (round.gameState === 'GameOver') {
        setGameOver(true)
        // Refresh the game so GamePage swaps to the result view.
        void queryClient.invalidateQueries({ queryKey: ['game', gameId] })
      }
    } catch (err) {
      setError(problemMessage(err, 'That attack was rejected.'))
    }
  }

  function opponentCellClass(code: string): string {
    const shot = battle.myShots[code]
    if (shot === 'Hit') return 'bg-red-600 border-red-400'
    if (shot === 'Missed') return 'bg-slate-600 border-slate-500'
    return gameOver ? 'bg-slate-800' : 'bg-slate-800 hover:bg-cyan-800 cursor-crosshair'
  }

  function myCellClass(code: string): string {
    const shot = battle.incomingShots[code]
    if (shot === 'Hit') return 'bg-red-600 border-red-400'
    if (shot === 'Missed') return 'bg-slate-600 border-slate-500'
    if (playerShips.has(code)) return 'bg-cyan-700 border-cyan-500'
    return 'bg-slate-800'
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
