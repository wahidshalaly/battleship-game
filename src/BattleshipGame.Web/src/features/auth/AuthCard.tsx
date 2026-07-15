import type { ReactNode } from 'react'
import { Link } from 'react-router'

interface AuthCardProps {
  title: string
  onSubmit: () => void
  error?: string | null
  submitting: boolean
  submitLabel: string
  footer: ReactNode
  children: ReactNode
}

export function AuthCard({
  title,
  onSubmit,
  error,
  submitting,
  submitLabel,
  footer,
  children,
}: AuthCardProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-900 p-4 text-slate-100">
      <form
        onSubmit={(e) => {
          e.preventDefault()
          onSubmit()
        }}
        className="w-full max-w-sm rounded-xl bg-slate-800 p-8 shadow-xl"
      >
        <h1 className="mb-6 text-2xl font-semibold">{title}</h1>
        <div className="flex flex-col gap-4">{children}</div>
        {error && (
          <p role="alert" className="mt-4 text-sm text-red-400">
            {error}
          </p>
        )}
        <button
          type="submit"
          disabled={submitting}
          className="mt-6 w-full rounded-lg bg-cyan-600 py-2 font-medium text-white transition hover:bg-cyan-500 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? 'Please wait…' : submitLabel}
        </button>
        <p className="mt-4 text-center text-sm text-slate-400">{footer}</p>
      </form>
    </div>
  )
}

interface FieldProps {
  label: string
  type?: string
  value: string
  onChange: (value: string) => void
  autoComplete?: string
  required?: boolean
}

export function Field({
  label,
  type = 'text',
  value,
  onChange,
  autoComplete,
  required,
}: FieldProps) {
  return (
    <label className="flex flex-col gap-1 text-sm">
      <span className="text-slate-300">{label}</span>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        autoComplete={autoComplete}
        required={required}
        className="rounded-lg border border-slate-600 bg-slate-900 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500"
      />
    </label>
  )
}

export const authLinkClass = 'font-medium text-cyan-400 hover:underline'
export { Link }
