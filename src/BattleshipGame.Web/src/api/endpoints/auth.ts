import { client, unwrap } from '../client'
import type { components } from '../schema'

type AuthTokenResponse = components['schemas']['AuthTokenResponse']

export function register(
  body: components['schemas']['RegisterRequest'],
): Promise<AuthTokenResponse> {
  return unwrap(client.POST('/api/auth/register', { body }))
}

export function signIn(body: components['schemas']['SignInRequest']): Promise<AuthTokenResponse> {
  return unwrap(client.POST('/api/auth/signin', { body }))
}

export function refresh(body: components['schemas']['RefreshRequest']): Promise<AuthTokenResponse> {
  return unwrap(client.POST('/api/auth/refresh', { body }))
}

export async function logout(body: components['schemas']['RefreshRequest']): Promise<void> {
  await unwrap(client.POST('/api/auth/logout', { body }))
}
