import { useAuth } from '../auth/AuthContext'

// Placeholder lobby — Phase 5 adds active-game resume and the new-game form.
export function LobbyPage() {
  const { player, isPlayerLoading, signOut } = useAuth()

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-slate-900 text-slate-100">
      <h1 className="text-3xl font-semibold">Battleship</h1>
      {isPlayerLoading ? (
        <p className="text-slate-400">Loading your profile…</p>
      ) : (
        <p className="text-slate-300">
          Signed in as <span className="font-medium">{player?.username ?? 'unknown'}</span>
        </p>
      )}
      <button
        type="button"
        onClick={() => void signOut()}
        className="rounded-lg bg-slate-700 px-4 py-2 font-medium text-white transition hover:bg-slate-600"
      >
        Sign out
      </button>
    </div>
  )
}
