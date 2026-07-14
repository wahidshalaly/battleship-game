import { client, unwrap } from '../client'
import type { components } from '../schema'

type PlayerResponse = components['schemas']['PlayerResponse']

export function getMe(): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/me'))
}

export function getPlayerById(id: string): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/{id}', { params: { path: { id } } }))
}

export function getPlayerByUsername(username: string): Promise<PlayerResponse> {
  return unwrap(client.GET('/api/players/{username}', { params: { path: { username } } }))
}
