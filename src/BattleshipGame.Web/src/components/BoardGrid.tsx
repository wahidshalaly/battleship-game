import type { ReactNode } from 'react'

const LETTERS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'

interface BoardGridProps {
  boardSize: number
  /** Extra classes for a given cell (background/border by state). */
  cellClassName?: (code: string) => string
  /** Optional content rendered inside a cell (e.g. hit/miss marker). */
  cellContent?: (code: string) => ReactNode
  onCellClick?: (code: string) => void
  onCellHover?: (code: string | null) => void
  disabled?: boolean
  label?: string
}

export function BoardGrid({
  boardSize,
  cellClassName,
  cellContent,
  onCellClick,
  onCellHover,
  disabled,
  label,
}: BoardGridProps) {
  const letters = LETTERS.slice(0, boardSize).split('')
  const digits = Array.from({ length: boardSize }, (_, i) => i + 1)

  return (
    <div>
      {label && <p className="mb-2 text-center text-sm font-medium text-slate-300">{label}</p>}
      <div
        className="inline-grid gap-0.5"
        style={{ gridTemplateColumns: `1.5rem repeat(${boardSize}, 2rem)` }}
        onMouseLeave={() => onCellHover?.(null)}
      >
        {/* Column headers */}
        <span />
        {letters.map((letter) => (
          <span key={letter} className="text-center text-xs text-slate-500">
            {letter}
          </span>
        ))}

        {digits.map((digit) => (
          <div key={digit} className="contents">
            <span className="flex items-center justify-end pr-1 text-xs text-slate-500">
              {digit}
            </span>
            {letters.map((letter) => {
              const code = `${letter}${digit}`
              return (
                <button
                  key={code}
                  type="button"
                  disabled={disabled}
                  onClick={() => onCellClick?.(code)}
                  onMouseEnter={() => onCellHover?.(code)}
                  className={`h-8 w-8 rounded-sm border border-slate-700 text-xs transition disabled:cursor-default ${
                    cellClassName?.(code) ?? 'bg-slate-800'
                  }`}
                >
                  {cellContent?.(code)}
                </button>
              )
            })}
          </div>
        ))}
      </div>
    </div>
  )
}
