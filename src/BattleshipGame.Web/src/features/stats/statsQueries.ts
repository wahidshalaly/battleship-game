import { useQuery } from '@tanstack/react-query'
import { getLeaderboard } from '../../api/endpoints/players'

export const LEADERBOARD_SIZE = 10

export function useLeaderboard(top = LEADERBOARD_SIZE) {
  return useQuery({
    queryKey: ['leaderboard', top],
    queryFn: () => getLeaderboard(top),
  })
}
