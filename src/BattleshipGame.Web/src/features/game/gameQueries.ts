import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as gamesApi from '../../api/endpoints/games'
import type { components } from '../../api/schema'

type CreateGameRequest = components['schemas']['CreateGameRequest']

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
