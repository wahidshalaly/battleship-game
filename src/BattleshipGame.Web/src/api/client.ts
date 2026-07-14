import createClient, { type Middleware } from 'openapi-fetch'
import type { paths } from './schema'
import {
  clearTokens,
  getAccessToken,
  getRefreshToken,
  setTokens,
} from '../features/auth/tokenStorage'

const baseUrl = import.meta.env.VITE_API_BASE_URL as string

export const client = createClient<paths>({ baseUrl })

const authMiddleware: Middleware = {
  onRequest({ request }) {
    const token = getAccessToken()
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`)
    }
    return request
  },
}

// Refreshes the access token once on a 401 and retries the original request; if the
// refresh itself fails, clears tokens and sends the user back to the login screen.
let refreshInFlight: Promise<boolean> | null = null

async function refreshTokens(): Promise<boolean> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) return false

  const response = await fetch(`${baseUrl}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  })
  if (!response.ok) return false

  const tokens = (await response.json()) as { accessToken: string; refreshToken: string }
  setTokens(tokens)
  return true
}

const retryOn401Middleware: Middleware = {
  async onResponse({ request, response }) {
    if (response.status !== 401) return response

    refreshInFlight ??= refreshTokens().finally(() => {
      refreshInFlight = null
    })
    const refreshed = await refreshInFlight

    if (!refreshed) {
      clearTokens()
      window.location.assign('/login')
      return response
    }

    const retryRequest = request.clone()
    retryRequest.headers.set('Authorization', `Bearer ${getAccessToken()}`)
    return fetch(retryRequest)
  },
}

client.use(authMiddleware, retryOn401Middleware)

export async function unwrap<R extends Promise<{ data?: unknown; error?: unknown }>>(
  promise: R,
): Promise<NonNullable<Awaited<R>['data']>> {
  const { data, error } = await promise
  if (error !== undefined) {
    throw error
  }
  return data as NonNullable<Awaited<R>['data']>
}
