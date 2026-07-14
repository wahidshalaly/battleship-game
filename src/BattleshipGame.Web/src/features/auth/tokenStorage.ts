// Tokens are kept in localStorage so a page reload doesn't log the user out, with an
// in-memory mirror to avoid a localStorage read on every request. This is a pragmatic
// choice for a learning project — it's vulnerable to token theft via XSS, unlike an
// httpOnly-cookie BFF proxy, which would be the production-grade alternative.
const ACCESS_TOKEN_KEY = 'bg_access_token'
const REFRESH_TOKEN_KEY = 'bg_refresh_token'

let accessToken: string | null = localStorage.getItem(ACCESS_TOKEN_KEY)
let refreshToken: string | null = localStorage.getItem(REFRESH_TOKEN_KEY)

export function getAccessToken(): string | null {
  return accessToken
}

export function getRefreshToken(): string | null {
  return refreshToken
}

export function setTokens(next: { accessToken: string; refreshToken: string }): void {
  accessToken = next.accessToken
  refreshToken = next.refreshToken
  localStorage.setItem(ACCESS_TOKEN_KEY, next.accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, next.refreshToken)
}

export function clearTokens(): void {
  accessToken = null
  refreshToken = null
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}
