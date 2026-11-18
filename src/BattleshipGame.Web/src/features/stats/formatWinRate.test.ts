import { describe, expect, it } from 'vitest'
import { formatWinRate } from './formatWinRate'

describe('formatWinRate', () => {
  it('formats a win rate as a whole percentage', () => {
    expect(formatWinRate(0.75)).toBe('75%')
    expect(formatWinRate(1)).toBe('100%')
  })

  it('rounds to the nearest percent', () => {
    expect(formatWinRate(2 / 3)).toBe('67%')
    expect(formatWinRate(1 / 3)).toBe('33%')
  })

  it('shows 0% when there is no win rate yet', () => {
    expect(formatWinRate(0)).toBe('0%')
    expect(formatWinRate(undefined)).toBe('0%')
  })
})
