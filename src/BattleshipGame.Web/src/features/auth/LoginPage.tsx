import { useState } from 'react'
import { useNavigate } from 'react-router'
import { problemMessage } from '../../api/problemDetails'
import { AuthCard, Field, Link, authLinkClass } from './AuthCard'
import { useAuth } from './AuthContext'

export function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit() {
    setError(null)
    setSubmitting(true)
    try {
      await signIn(username, password)
      navigate('/')
    } catch (err) {
      setError(problemMessage(err, 'Sign in failed. Check your username and password.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthCard
      title="Sign in"
      onSubmit={handleSubmit}
      error={error}
      submitting={submitting}
      submitLabel="Sign in"
      footer={
        <>
          No account?{' '}
          <Link to="/register" className={authLinkClass}>
            Register
          </Link>
        </>
      }
    >
      <Field
        label="Username"
        value={username}
        onChange={setUsername}
        autoComplete="username"
        required
      />
      <Field
        label="Password"
        type="password"
        value={password}
        onChange={setPassword}
        autoComplete="current-password"
        required
      />
    </AuthCard>
  )
}
