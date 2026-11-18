import type { components } from '../../api/schema'
import { formatWinRate } from './formatWinRate'

type PlayerResponse = components['schemas']['PlayerResponse']

/** The signed-in player's own record: wins, losses and win rate over finished games. */
export function PlayerRecord({ player }: { player: PlayerResponse | undefined }) {
  const stats = [
    { label: 'Won', value: String(player?.gamesWon ?? 0) },
    { label: 'Lost', value: String(player?.gamesLost ?? 0) },
    { label: 'Win rate', value: formatWinRate(player?.winRate) },
  ]

  return (
    <section aria-labelledby="player-record-heading" className="rounded-xl bg-slate-800 p-6">
      <h2 id="player-record-heading" className="mb-4 text-lg font-semibold">
        Your record
      </h2>
      <dl className="grid grid-cols-3 gap-4 text-center">
        {stats.map((stat) => (
          <div key={stat.label}>
            <dt className="text-xs tracking-wide text-slate-400 uppercase">{stat.label}</dt>
            <dd className="mt-1 text-2xl font-semibold tabular-nums">{stat.value}</dd>
          </div>
        ))}
      </dl>
      <p className="mt-4 text-sm text-slate-400">
        {player?.totalGamesPlayed ?? 0} games played, including any game in progress.
      </p>
    </section>
  )
}
