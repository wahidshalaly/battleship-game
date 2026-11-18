import { client, unwrap } from '../client'
import type { components } from '../schema'

type PlayerResponse = components['schemas']['PlayerResponse']
type LeaderboardResponse = components['schemas']['LeaderboardResponse']

export function getMe(): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/me'))
}

export function getPlayerById(id: string): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/{id}', { params: { path: { id } } }))
}

export function getPlayerByUsername(username: string): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/{username}', { params: { path: { username } } }))
}

export function getLeaderboard(top: number): Promise<LeaderboardResponse> {
  return unwrap(client.GET('/api/players/leaderboard', { params: { query: { top } } }))
}
