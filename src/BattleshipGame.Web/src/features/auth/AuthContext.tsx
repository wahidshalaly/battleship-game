import { createContext, use, useState, type ReactNode } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import * as authApi from '../../api/endpoints/auth'
import { getMe } from '../../api/endpoints/players'
import type { components } from '../../api/schema'
import { clearTokens, getAccessToken, getRefreshToken, setTokens } from './tokenStorage'

type PlayerResponse = components['schemas']['PlayerResponse']
type AuthTokenResponse = components['schemas']['AuthTokenResponse']

interface AuthContextValue {
  isAuthenticated: boolean
  player: PlayerResponse | undefined
  isPlayerLoading: boolean
  signIn: (username: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function storeTokens(tokens: AuthTokenResponse): void {
  setTokens({
    accessToken: tokens.accessToken ?? '',
    refreshToken: tokens.refreshToken ?? '',
  })
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [isAuthenticated, setIsAuthenticated] = useState(() => getAccessToken() !== null)

  // Load the authenticated player's profile once signed in.
  const { data: player, isLoading: isPlayerLoading } = useQuery({
    queryKey: ['player', 'me'],
    queryFn: getMe,
    enabled: isAuthenticated,
  })

  async function signIn(username: string, password: string): Promise<void> {
    storeTokens(await authApi.signIn({ username, password }))
    setIsAuthenticated(true)
  }

  async function register(username: string, email: string, password: string): Promise<void> {
    storeTokens(await authApi.register({ username, email, password }))
    setIsAuthenticated(true)
  }

  async function signOut(): Promise<void> {
    const refreshToken = getRefreshToken()
    try {
      if (refreshToken) await authApi.logout({ refreshToken })
    } finally {
      clearTokens()
      setIsAuthenticated(false)
      queryClient.clear()
    }
  }

  return (
    <AuthContext value={{ isAuthenticated, player, isPlayerLoading, signIn, register, signOut }}>
      {children}
    </AuthContext>
  )
}

export function useAuth(): AuthContextValue {
  const context = use(AuthContext)
  if (context === null) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
