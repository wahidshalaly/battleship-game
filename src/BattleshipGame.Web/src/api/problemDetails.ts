export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | null
  detail?: string | null
  instance?: string | null
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null && ('title' in value || 'detail' in value)
}

export function problemMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (isProblemDetails(error)) {
    return error.detail ?? error.title ?? fallback
  }
  if (error instanceof Error) {
    return error.message
  }
  return fallback
}
