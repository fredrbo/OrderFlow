import { formatTime } from '@/lib/format'
import { cn } from '@/lib/utils'

interface LiveUpdateIndicatorProps {
  updatedAt: number
  hasError: boolean
}

export function LiveUpdateIndicator({ updatedAt, hasError }: LiveUpdateIndicatorProps) {
  const label = hasError
    ? 'Falha ao atualizar. Tentando novamente…'
    : updatedAt
      ? `Atualizado às ${formatTime(updatedAt)}`
      : 'Carregando…'

  return (
    <span className="inline-flex items-center gap-2 text-xs text-muted-foreground">
      <span className="relative flex size-2" aria-hidden>
        {!hasError && updatedAt > 0 && (
          <span
            key={updatedAt}
            className="absolute inline-flex size-full rounded-full bg-emerald-500 opacity-75 animate-[ping_1s_cubic-bezier(0,0,0.2,1)]"
          />
        )}
        <span
          className={cn(
            'relative inline-flex size-2 rounded-full',
            hasError ? 'bg-destructive' : updatedAt ? 'bg-emerald-500' : 'bg-muted-foreground/40',
          )}
        />
      </span>
      <span className="tabular-nums">{label}</span>
    </span>
  )
}
