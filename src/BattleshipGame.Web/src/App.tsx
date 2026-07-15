import { Navigate, Route, Routes } from 'react-router'
import { LoginPage } from './features/auth/LoginPage'
import { RegisterPage } from './features/auth/RegisterPage'
import { LobbyPage } from './features/game/LobbyPage'
import { ProtectedRoute } from './routes/ProtectedRoute'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<LobbyPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
