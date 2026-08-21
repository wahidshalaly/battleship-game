import { useState } from 'react'
import { useNavigate } from 'react-router'
import { problemMessage } from '../../api/problemDetails'
import { AuthCard, Field, Link, authLinkClass } from './AuthCard'
import { useAuth } from './AuthContext'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit() {
    setError(null)
    setSubmitting(true)
    try {
      await register(username, email, password)
      navigate('/')
    } catch (err) {
      setError(problemMessage(err, 'Registration failed. Please try again.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthCard
      title="Create account"
      onSubmit={handleSubmit}
      error={error}
      submitting={submitting}
      submitLabel="Register"
      footer={
        <>
          Already have an account?{' '}
          <Link to="/login" className={authLinkClass}>
            Sign in
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
        label="Email"
        type="email"
        value={email}
        onChange={setEmail}
        autoComplete="email"
        required
      />
      <Field
        label="Password"
        type="password"
        value={password}
        onChange={setPassword}
        autoComplete="new-password"
        required
      />
    </AuthCard>
  )
}
