import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { components } from '../../api/schema'
import { LeaderboardPanel } from './LeaderboardPanel'

type LeaderboardResponse = components['schemas']['LeaderboardResponse']

const leaderboard: LeaderboardResponse = {
  minimumFinishedGamesToRank: 5,
  entries: [
    {
      rank: 1,
      playerId: 'p1',
      username: 'admiral',
      gamesFinished: 8,
      gamesWon: 6,
      aiGamesWon: 2,
      winRate: 0.75,
      isProvisional: false,
    },
    {
      rank: 2,
      playerId: 'p2',
      username: 'cadet',
      gamesFinished: 1,
      gamesWon: 1,
      aiGamesWon: 0,
      winRate: 1,
      isProvisional: true,
    },
  ],
}

function renderPanel(overrides: Partial<Parameters<typeof LeaderboardPanel>[0]> = {}) {
  return render(
    <LeaderboardPanel
      leaderboard={leaderboard}
      isLoading={false}
      isError={false}
      currentPlayerId={undefined}
      {...overrides}
    />,
  )
}

describe('LeaderboardPanel', () => {
  it('lists the players in rank order with their statistics', () => {
    renderPanel()

    const rows = within(screen.getByRole('table')).getAllByRole('row').slice(1)
    const cells = rows.map((row) =>
      within(row)
        .getAllByRole('cell')
        .map((cell) => cell.textContent),
    )
    expect(cells).toEqual([
      ['1', 'admiral', '8', '6', '2', '75%'],
      ['2', 'cadetprovisional', '1', '1', '0', '100%'],
    ])
  })

  it('marks only players below the minimum as provisional', () => {
    renderPanel()

    expect(screen.getAllByText('provisional')).toHaveLength(1)
    expect(screen.getByText(/ranked by win rate after 5 finished games/i)).toBeInTheDocument()
  })

  it('highlights the signed-in player', () => {
    renderPanel({ currentPlayerId: 'p2' })

    const current = screen.getByRole('row', { current: true })
    expect(current).toHaveTextContent('cadet')
  })

  it('invites players to win a game when nobody has finished one', () => {
    renderPanel({ leaderboard: { minimumFinishedGamesToRank: 5, entries: [] } })

    expect(screen.queryByRole('table')).not.toBeInTheDocument()
    expect(screen.getByText(/no finished games yet/i)).toBeInTheDocument()
  })

  it('reports a failure to load', () => {
    renderPanel({ leaderboard: undefined, isError: true })

    expect(screen.getByRole('alert')).toHaveTextContent(/could not load the leaderboard/i)
  })
})
