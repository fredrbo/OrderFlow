import { useEffect, useRef, useState } from 'react'
import { toast } from 'sonner'
import type { Order, OrderStatus } from '../types'

const HIGHLIGHT_DURATION_MS = 3_000

export function useOrderStatusWatcher(orders: Order[] | undefined): ReadonlySet<string> {
  const previousStatuses = useRef<Map<string, OrderStatus> | null>(null)
  const timers = useRef(new Set<ReturnType<typeof setTimeout>>())
  const [highlighted, setHighlighted] = useState<ReadonlySet<string>>(new Set())

  useEffect(() => {
    const pending = timers.current
    return () => pending.forEach(clearTimeout)
  }, [])

  useEffect(() => {
    if (!orders) return

    const previous = previousStatuses.current
    previousStatuses.current = new Map(orders.map((o) => [o.id, o.status]))

    if (!previous) return

    const changed = orders.filter((o) => previous.has(o.id) && previous.get(o.id) !== o.status)
    if (changed.length === 0) return

    for (const order of changed) {
      toast.info(`Pedido de ${order.cliente} mudou de status`, {
        description: `${previous.get(order.id)} → ${order.status}`,
      })
    }

    const ids = changed.map((o) => o.id)
    setHighlighted((current) => new Set([...current, ...ids]))

    const timer = setTimeout(() => {
      timers.current.delete(timer)
      setHighlighted((current) => new Set([...current].filter((id) => !ids.includes(id))))
    }, HIGHLIGHT_DURATION_MS)
    timers.current.add(timer)
  }, [orders])

  return highlighted
}
