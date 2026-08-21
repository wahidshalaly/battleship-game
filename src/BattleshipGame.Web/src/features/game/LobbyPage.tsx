import { useState } from 'react'
import { useNavigate } from 'react-router'
import { problemMessage } from '../../api/problemDetails'
import type { components } from '../../api/schema'
import { PageShell } from '../../components/PageShell'
import { useActiveGame, useCreateGame } from './gameQueries'

type OpponentStrategy = components['schemas']['OpponentStrategy']

const OPPONENTS: { value: OpponentStrategy; label: string }[] = [
  { value: 'Random', label: 'Random (fast)' },
  { value: 'SemanticKernel', label: 'AI (Semantic Kernel)' },
]

export function LobbyPage() {
  const navigate = useNavigate()
  const { data: activeGame, isLoading } = useActiveGame()
  const createGame = useCreateGame()
  const [opponent, setOpponent] = useState<OpponentStrategy>('Random')
  const [error, setError] = useState<string | null>(null)

  async function handleCreate() {
    setError(null)
    try {
      const gameId = await createGame.mutateAsync({ boardSize: 10, opponentStrategy: opponent })
      navigate(`/games/${gameId}`)
    } catch (err) {
      setError(problemMessage(err, 'Could not create a game.'))
    }
  }

  return (
    <PageShell>
      {isLoading ? (
        <p className="text-slate-400">Loading…</p>
      ) : activeGame?.gameId ? (
        <section className="rounded-xl bg-slate-800 p-8">
          <h2 className="mb-2 text-xl font-semibold">Resume your game</h2>
          <p className="mb-6 text-slate-400">You have a game in progress ({activeGame.state}).</p>
          <button
            type="button"
            onClick={() => navigate(`/games/${activeGame.gameId}`)}
            className="rounded-lg bg-cyan-600 px-5 py-2 font-medium text-white transition hover:bg-cyan-500"
          >
            Resume game
          </button>
        </section>
      ) : (
        <section className="rounded-xl bg-slate-800 p-8">
          <h2 className="mb-6 text-xl font-semibold">New game</h2>
          <label className="mb-6 flex max-w-xs flex-col gap-1 text-sm">
            <span className="text-slate-300">Opponent</span>
            <select
              value={opponent}
              onChange={(e) => setOpponent(e.target.value as OpponentStrategy)}
              className="rounded-lg border border-slate-600 bg-slate-900 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500"
            >
              {OPPONENTS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </label>
          {error && (
            <p role="alert" className="mb-4 text-sm text-red-400">
              {error}
            </p>
          )}
          <button
            type="button"
            onClick={() => void handleCreate()}
            disabled={createGame.isPending}
            className="rounded-lg bg-cyan-600 px-5 py-2 font-medium text-white transition hover:bg-cyan-500 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {createGame.isPending ? 'Creating…' : 'Create game'}
          </button>
        </section>
      )}
    </PageShell>
  )
}
