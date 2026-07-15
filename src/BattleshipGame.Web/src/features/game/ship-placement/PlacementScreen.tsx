import { useMemo, useState } from 'react'
import { problemMessage } from '../../../api/problemDetails'
import type { components } from '../../../api/schema'
import { BoardGrid } from '../../../components/BoardGrid'
import { usePlaceShip, useStartGame } from '../gameQueries'
import { cellsForShip, fits, generateRandomPlacements } from './placementLogic'
import { SHIP_KINDS, SHIP_LENGTHS, type PlaceableShipKind } from './shipLengths'

type ShipOrientation = components['schemas']['ShipOrientation']

interface PlacementScreenProps {
  gameId: string
  boardSize: number
}

export function PlacementScreen({ gameId, boardSize }: PlacementScreenProps) {
  const placeShip = usePlaceShip(gameId)
  const startGame = useStartGame(gameId)

  const [placed, setPlaced] = useState<Record<string, string[]>>({})
  const [armed, setArmed] = useState<PlaceableShipKind>(SHIP_KINDS[0])
  const [orientation, setOrientation] = useState<ShipOrientation>('Horizontal')
  const [hover, setHover] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const occupied = useMemo(() => new Set(Object.values(placed).flat()), [placed])
  const remaining = SHIP_KINDS.filter((k) => !(k in placed))
  const allPlaced = remaining.length === 0

  // Cells the armed ship would occupy under the cursor (for a placement preview).
  const preview = useMemo(() => {
    if (allPlaced || hover === null) return null
    const cells = cellsForShip(armed, orientation, hover, boardSize)
    return cells && fits(cells, occupied) ? cells : null
  }, [allPlaced, hover, armed, orientation, boardSize, occupied])

  const previewSet = useMemo(() => new Set(preview ?? []), [preview])

  async function handleCellClick(bowCode: string) {
    setError(null)
    if (allPlaced) return

    const cells = cellsForShip(armed, orientation, bowCode, boardSize)
    if (!cells) {
      setError(`${armed} doesn't fit there — it would run off the board.`)
      return
    }
    if (!fits(cells, occupied)) {
      setError(`${armed} would overlap another ship.`)
      return
    }

    try {
      await placeShip.mutateAsync({
        side: 'Player',
        shipKind: armed,
        orientation,
        bowCode,
      })
      setPlaced((prev) => {
        const next = { ...prev, [armed]: cells }
        const stillRemaining = SHIP_KINDS.filter((k) => !(k in next))
        if (stillRemaining.length > 0) setArmed(stillRemaining[0])
        return next
      })
    } catch (err) {
      setError(problemMessage(err, 'The server rejected that placement.'))
    }
  }

  function cellClassName(code: string): string {
    if (occupied.has(code)) return 'bg-cyan-700 border-cyan-500'
    if (previewSet.has(code)) return 'bg-cyan-900 border-cyan-600'
    return 'bg-slate-800 hover:bg-slate-700'
  }

  return (
    <div className="flex flex-col gap-8 lg:flex-row">
      <div>
        <h2 className="mb-4 text-xl font-semibold">Place your ships</h2>
        <BoardGrid
          boardSize={boardSize}
          cellClassName={cellClassName}
          onCellClick={(code) => void handleCellClick(code)}
          onCellHover={setHover}
          disabled={allPlaced || placeShip.isPending}
        />
        {error && (
          <p role="alert" className="mt-4 text-sm text-red-400">
            {error}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-4">
        <div>
          <h3 className="mb-2 text-sm font-medium text-slate-300">Orientation</h3>
          <div className="flex gap-2">
            {(['Horizontal', 'Vertical'] as const).map((o) => (
              <button
                key={o}
                type="button"
                onClick={() => setOrientation(o)}
                className={`rounded-md px-3 py-1.5 text-sm font-medium transition ${
                  orientation === o
                    ? 'bg-cyan-600 text-white'
                    : 'bg-slate-700 text-slate-200 hover:bg-slate-600'
                }`}
              >
                {o}
              </button>
            ))}
          </div>
        </div>

        <div>
          <h3 className="mb-2 text-sm font-medium text-slate-300">Ships</h3>
          <ul className="flex flex-col gap-1">
            {SHIP_KINDS.map((kind) => {
              const isPlaced = kind in placed
              const isArmed = kind === armed && !isPlaced
              return (
                <li key={kind}>
                  <button
                    type="button"
                    disabled={isPlaced}
                    onClick={() => setArmed(kind)}
                    className={`flex w-56 items-center justify-between rounded-md px-3 py-2 text-sm transition ${
                      isPlaced
                        ? 'bg-slate-800 text-slate-500 line-through'
                        : isArmed
                          ? 'bg-cyan-600 text-white'
                          : 'bg-slate-700 text-slate-200 hover:bg-slate-600'
                    }`}
                  >
                    <span>{kind}</span>
                    <span className="text-xs opacity-75">{SHIP_LENGTHS[kind]} cells</span>
                  </button>
                </li>
              )
            })}
          </ul>
        </div>

        {allPlaced && (
          <button
            type="button"
            onClick={() => startGame.mutate(generateRandomPlacements(boardSize))}
            disabled={startGame.isPending}
            className="mt-2 rounded-lg bg-emerald-600 px-5 py-2 font-medium text-white transition hover:bg-emerald-500 disabled:opacity-50"
          >
            {startGame.isPending ? 'Starting…' : 'Start game'}
          </button>
        )}
        {startGame.isError && (
          <p role="alert" className="text-sm text-red-400">
            {problemMessage(startGame.error, 'Could not start the game.')}
          </p>
        )}
      </div>
    </div>
  )
}
