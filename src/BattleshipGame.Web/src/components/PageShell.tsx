import type { ReactNode } from 'react'
import { useAuth } from '../features/auth/AuthContext'

export function PageShell({ children }: { children: ReactNode }) {
  const { player, signOut } = useAuth()

  return (
    <div className="min-h-screen bg-slate-900 text-slate-100">
      <header className="flex items-center justify-between border-b border-slate-700 px-6 py-3">
        <span className="text-lg font-semibold tracking-wide">Battleship</span>
        <div className="flex items-center gap-4 text-sm">
          {player?.username && <span className="text-slate-400">{player.username}</span>}
          <button
            type="button"
            onClick={() => void signOut()}
            className="rounded-md bg-slate-700 px-3 py-1.5 font-medium transition hover:bg-slate-600"
          >
            Sign out
          </button>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-6 py-8">{children}</main>
    </div>
  )
}
