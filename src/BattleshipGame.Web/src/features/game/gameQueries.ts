import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as gamesApi from '../../api/endpoints/games'
import type { components } from '../../api/schema'
import type { ShipPlacement } from './ship-placement/placementLogic'

type CreateGameRequest = components['schemas']['CreateGameRequest']
type PlaceShipRequest = components['schemas']['PlaceShipRequest']

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
  return useMutation({
    mutationFn: (body: PlaceShipRequest) => gamesApi.placeShip(gameId, body),
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
