import { cn } from '@/lib/utils'
import { formatDuration, formatTime } from '@/lib/format'
import type { OrderStatus, OrderStatusHistoryEntry } from '../types'

const dotClassName: Record<OrderStatus, string> = {
  Pendente: 'bg-amber-500',
  Processando: 'bg-sky-500',
  Finalizado: 'bg-emerald-500',
}

export function OrderStatusTimeline({ historico }: { historico: OrderStatusHistoryEntry[] }) {
  if (historico.length === 0) return null

  return (
    <section className="border-t pt-4">
      <h3 className="mb-3 text-sm font-medium">Histórico de status</h3>
      <ol className="grid gap-3">
        {historico.map((entry, index) => {
          const changedAt = new Date(entry.data_alteracao)
          const previous = historico[index - 1]
          const elapsed = previous && changedAt.getTime() - new Date(previous.data_alteracao).getTime()
          const isLast = index === historico.length - 1

          return (
            <li key={`${entry.status}-${entry.data_alteracao}`} className="relative flex items-start gap-3 text-sm">
              {!isLast && <span className="absolute top-3 left-[4.5px] h-full w-px bg-border" aria-hidden />}
              <span className={cn('mt-1.5 size-2.5 shrink-0 rounded-full', dotClassName[entry.status])} aria-hidden />
              <span className="flex-1 font-medium">{entry.status}</span>
              <span className="text-right text-muted-foreground tabular-nums">
                {formatTime(changedAt)}
                {elapsed !== undefined && <span className="block text-xs">+{formatDuration(elapsed)}</span>}
              </span>
            </li>
          )
        })}
      </ol>
    </section>
  )
}
