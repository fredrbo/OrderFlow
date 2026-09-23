import { CheckCircle2, Clock, Loader2, type LucideIcon } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import type { OrderStatus } from '../types'

const statusConfig: Record<OrderStatus, { icon: LucideIcon; className: string; iconClassName?: string }> = {
  Pendente: {
    icon: Clock,
    className: 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300',
  },
  Processando: {
    icon: Loader2,
    className: 'bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300',
    iconClassName: 'animate-spin',
  },
  Finalizado: {
    icon: CheckCircle2,
    className: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300',
  },
}

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  const { icon: Icon, className, iconClassName } = statusConfig[status]

  return (
    <Badge className={className}>
      <Icon className={cn(iconClassName)} aria-hidden />
      {status}
    </Badge>
  )
}
