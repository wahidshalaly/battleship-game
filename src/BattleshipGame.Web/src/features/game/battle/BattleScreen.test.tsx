import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as gamesApi from '../../../api/endpoints/games'
import { BattleScreen } from './BattleScreen'

vi.mock('../../../api/endpoints/games')

// A fresh QueryClient, as after signing out (which clears the cache) and signing in again:
// everything drawn has to come from the boards the API returns.
function renderResumedBattle() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <BattleScreen gameId="g1" boardSize={10} />
    </QueryClientProvider>,
  )
}

describe('BattleScreen when a game is resumed', () => {
  beforeEach(() => {
    vi.mocked(gamesApi.getGameBoards).mockResolvedValue({
      gameId: 'g1',
      ownBoard: {
        ships: [{ kind: 'Destroyer', position: ['A1', 'A2'], sunk: false }],
        hits: ['A1'],
        misses: ['J10'],
      },
      opponentBoard: { ships: [], hits: ['B2'], misses: ['C3'] },
    })
  })

  it('draws the earlier hits and misses on both boards, and the own ships', async () => {
    renderResumedBattle()

    const opponent = within(await screen.findByRole('group', { name: /opponent/i }))
    const own = within(screen.getByRole('group', { name: /your fleet/i }))
    expect(gamesApi.getGameBoards).toHaveBeenCalledWith('g1')
    expect(opponent.getByRole('button', { name: 'B2' })).toHaveClass('bg-red-600')
    expect(opponent.getByRole('button', { name: 'C3' })).toHaveClass('bg-slate-600')
    expect(own.getByRole('button', { name: 'A1' })).toHaveClass('bg-red-600')
    expect(own.getByRole('button', { name: 'J10' })).toHaveClass('bg-slate-600')
    expect(own.getByRole('button', { name: 'A2' })).toHaveClass('bg-cyan-700')
  })

  it('does not attack a cell that was attacked before signing out', async () => {
    const user = userEvent.setup()
    renderResumedBattle()

    const opponent = within(await screen.findByRole('group', { name: /opponent/i }))
    await user.click(opponent.getByRole('button', { name: 'B2' }))
    await user.click(opponent.getByRole('button', { name: 'C3' }))

    expect(gamesApi.attack).not.toHaveBeenCalled()
  })
})
