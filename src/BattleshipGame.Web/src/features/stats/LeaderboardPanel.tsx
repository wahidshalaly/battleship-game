import type { components } from '../../api/schema'
import { formatWinRate } from './formatWinRate'

type LeaderboardResponse = components['schemas']['LeaderboardResponse']

interface LeaderboardPanelProps {
  leaderboard: LeaderboardResponse | undefined
  isLoading: boolean
  isError: boolean
  currentPlayerId: string | undefined
}

/** The wall of fame: the top players, best first, with the signed-in player highlighted. */
export function LeaderboardPanel({
  leaderboard,
  isLoading,
  isError,
  currentPlayerId,
}: LeaderboardPanelProps) {
  const entries = leaderboard?.entries ?? []
  const minimumGames = leaderboard?.minimumFinishedGamesToRank

  return (
    <section aria-labelledby="leaderboard-heading" className="rounded-xl bg-slate-800 p-6">
      <h2 id="leaderboard-heading" className="mb-1 text-lg font-semibold">
        Leaderboard
      </h2>
      {minimumGames !== undefined && (
        <p className="mb-4 text-sm text-slate-400">
          Ranked by win rate after {minimumGames} finished games; wins against the AI break ties.
        </p>
      )}
      {isLoading ? (
        <p className="text-slate-400">Loading…</p>
      ) : isError ? (
        <p role="alert" className="text-sm text-red-400">
          Could not load the leaderboard.
        </p>
      ) : entries.length === 0 ? (
        <p className="text-slate-400">No finished games yet. Win one to claim the top spot!</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs tracking-wide text-slate-400 uppercase">
              <tr>
                <th scope="col" className="py-2 pr-3">
                  #
                </th>
                <th scope="col" className="py-2 pr-3">
                  Player
                </th>
                <th scope="col" className="py-2 pr-3 text-right">
                  Played
                </th>
                <th scope="col" className="py-2 pr-3 text-right">
                  Won
                </th>
                <th scope="col" className="py-2 pr-3 text-right">
                  vs AI
                </th>
                <th scope="col" className="py-2 text-right">
                  Win rate
                </th>
              </tr>
            </thead>
            <tbody className="tabular-nums">
              {entries.map((entry) => (
                <tr
                  key={entry.playerId}
                  aria-current={entry.playerId === currentPlayerId ? 'true' : undefined}
                  className={`border-t border-slate-700 ${
                    entry.playerId === currentPlayerId ? 'bg-cyan-950 text-cyan-100' : ''
                  }`}
                >
                  <td className="py-2 pr-3">{entry.rank}</td>
                  <td className="py-2 pr-3">
                    {entry.username}
                    {entry.isProvisional && (
                      <span
                        title={`Fewer than ${minimumGames ?? 'the minimum'} finished games`}
                        className="ml-2 rounded bg-slate-700 px-1.5 py-0.5 text-xs text-slate-300"
                      >
                        provisional
                      </span>
                    )}
                  </td>
                  <td className="py-2 pr-3 text-right">{entry.gamesFinished}</td>
                  <td className="py-2 pr-3 text-right">{entry.gamesWon}</td>
                  <td className="py-2 pr-3 text-right">{entry.aiGamesWon}</td>
                  <td className="py-2 text-right">{formatWinRate(entry.winRate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
