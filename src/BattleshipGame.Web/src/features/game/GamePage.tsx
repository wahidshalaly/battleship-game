import { useNavigate, useParams } from 'react-router'
import { problemMessage } from '../../api/problemDetails'
import { PageShell } from '../../components/PageShell'
import { BattleScreen } from './battle/BattleScreen'
import { useGame } from './gameQueries'
import { PlacementScreen } from './ship-placement/PlacementScreen'

export function GamePage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: game, isLoading, error } = useGame(id!)

  if (isLoading) {
    return (
      <PageShell>
        <p className="text-slate-400">Loading game…</p>
      </PageShell>
    )
  }

  if (error || !game) {
    return (
      <PageShell>
        <p role="alert" className="text-red-400">
          {problemMessage(error, 'Game not found.')}
        </p>
        <button
          type="button"
          onClick={() => navigate('/')}
          className="mt-4 rounded-lg bg-slate-700 px-4 py-2 font-medium transition hover:bg-slate-600"
        >
          Back to lobby
        </button>
      </PageShell>
    )
  }

  return (
    <PageShell>
      {(game.state === 'New' || game.state === 'Ready') && (
        <PlacementScreen gameId={game.gameId!} boardSize={game.boardSize ?? 10} />
      )}
      {game.state === 'Started' && (
        <BattleScreen gameId={game.gameId!} boardSize={game.boardSize ?? 10} />
      )}
      {game.state === 'GameOver' && (
        <section className="rounded-xl bg-slate-800 p-8">
          <h2 className="mb-2 text-2xl font-semibold">
            {game.winnerSide === 'Player' ? 'You win! 🎉' : 'You lose'}
          </h2>
          <button
            type="button"
            onClick={() => navigate('/')}
            className="mt-4 rounded-lg bg-cyan-600 px-5 py-2 font-medium text-white transition hover:bg-cyan-500"
          >
            Back to lobby
          </button>
        </section>
      )}
    </PageShell>
  )
}
