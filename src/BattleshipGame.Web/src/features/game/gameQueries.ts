import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as gamesApi from '../../api/endpoints/games'
import type { components } from '../../api/schema'
import { applyRound, type GameBoards } from './battle/battleState'
import type { ShipPlacement } from './ship-placement/placementLogic'

type CreateGameRequest = components['schemas']['CreateGameRequest']
type PlaceShipRequest = components['schemas']['PlaceShipRequest']
type ShipKind = components['schemas']['ShipKind']

const boardsKey = (gameId: string) => ['game', gameId, 'boards']

function isShipKind(kind: ShipKind | null | undefined): boolean {
  return !!kind && kind !== 'None'
}

export function useActiveGame() {
  return useQuery({
    queryKey: ['game', 'active'],
    queryFn: gamesApi.getActiveGame,
  })
}

export function useGame(id: string) {
  return useQuery({
    queryKey: ['game', id],
    queryFn: () => gamesApi.getGame(id),
  })
}

/** The player's view of both boards: the source of truth for placement and battle screens. */
export function useGameBoards(gameId: string) {
  return useQuery({
    queryKey: boardsKey(gameId),
    queryFn: () => gamesApi.getGameBoards(gameId),
  })
}

export function useCreateGame() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateGameRequest) => gamesApi.createGame(body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['game', 'active'] })
    },
  })
}

export function usePlaceShip(gameId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: PlaceShipRequest) => gamesApi.placeShip(gameId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: boardsKey(gameId) }),
  })
}

export function useAttack(gameId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (cell: string) => gamesApi.attack(gameId, { cell }),
    onSuccess: async (round) => {
      // Stops an in-flight refetch from overwriting the round with older boards.
      await queryClient.cancelQueries({ queryKey: boardsKey(gameId) })
      queryClient.setQueryData<GameBoards>(
        boardsKey(gameId),
        (boards) => boards && applyRound(boards, round),
      )
      // The round names a sunk ship but not its cells, so refetch to show where it was.
      if (isShipKind(round.playerSunkShip) || isShipKind(round.opponentSunkShip)) {
        void queryClient.invalidateQueries({ queryKey: boardsKey(gameId) })
      }
    },
  })
}

/**
 * Places the opponent's ships (the API requires the client to set up both boards), then
 * transitions the game to Started. The opponent placements are generated randomly by the
 * caller so the player never sees them.
 */
export function useStartGame(gameId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (opponentPlacements: ShipPlacement[]) => {
      for (const placement of opponentPlacements) {
        await gamesApi.placeShip(gameId, { side: 'Opponent', ...placement })
      }
      return gamesApi.updateGameState(gameId, { state: 'Started' })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['game', gameId] })
    },
  })
}
