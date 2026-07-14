import { client, unwrap } from '../client'
import type { components } from '../schema'

type GetGameQueryResult = components['schemas']['GetGameQueryResult']
type LastRoundResult = components['schemas']['LastRoundResult']
type GameStateResponse = components['schemas']['GameStateResponse']

export function createGame(body: components['schemas']['CreateGameRequest']): Promise<string> {
  return unwrap(client.POST('/api/games', { body }))
}

export function getGame(id: string): Promise<GetGameQueryResult> {
  return unwrap(client.GET('/api/games/{id}', { params: { path: { id } } }))
}

export async function getActiveGame(): Promise<GetGameQueryResult | null> {
  const { data, error, response } = await client.GET('/api/games/active')
  if (response.status === 204) return null
  if (error !== undefined) throw error
  return data ?? null
}

export function placeShip(
  id: string,
  body: components['schemas']['PlaceShipRequest'],
): Promise<string> {
  return unwrap(client.POST('/api/games/{id}/ships', { params: { path: { id } }, body }))
}

export function attack(
  id: string,
  body: components['schemas']['AttackRequest'],
): Promise<LastRoundResult> {
  return unwrap(client.POST('/api/games/{id}/attacks', { params: { path: { id } }, body }))
}

export function updateGameState(
  id: string,
  body: components['schemas']['UpdateGameStateRequest'],
): Promise<GameStateResponse> {
  return unwrap(client.PUT('/api/games/{id}/state', { params: { path: { id } }, body }))
}

export function getGameState(id: string): Promise<GameStateResponse> {
  return unwrap(client.GET('/api/games/{id}/state', { params: { path: { id } } }))
}
