import { ChevronRight, PackageOpen } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatCurrency, formatDateTime } from '@/lib/format'
import { cn } from '@/lib/utils'
import type { Order } from '../types'
import { OrderStatusBadge } from './OrderStatusBadge'

interface OrdersTableProps {
  orders: Order[] | undefined
  isLoading: boolean
  highlightedIds: ReadonlySet<string>
  onSelect: (id: string) => void
}

export function OrdersTable({ orders, isLoading, highlightedIds, onSelect }: OrdersTableProps) {
  if (!isLoading && orders?.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 py-12 text-center text-muted-foreground">
        <PackageOpen className="size-10" aria-hidden />
        <p className="font-medium text-foreground">Nenhum pedido ainda</p>
        <p className="text-sm">Clique em “Novo pedido” para criar o primeiro.</p>
      </div>
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Cliente</TableHead>
          <TableHead className="hidden sm:table-cell">Produto</TableHead>
          <TableHead className="text-right">Valor</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="hidden md:table-cell">Criado em</TableHead>
          <TableHead className="hidden w-px sm:table-cell">
            <span className="sr-only">Ações</span>
          </TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {isLoading
          ? Array.from({ length: 4 }, (_, i) => (
              <TableRow key={i}>
                <TableCell colSpan={6}>
                  <Skeleton className="h-6 w-full" />
                </TableCell>
              </TableRow>
            ))
          : orders?.map((order) => (
              <TableRow
                key={order.id}
                onClick={() => onSelect(order.id)}
                className={cn(
                  'cursor-pointer transition-colors duration-700',
                  highlightedIds.has(order.id) && 'bg-sky-100/70 dark:bg-sky-500/15',
                )}
              >
                <TableCell className="font-medium">
                  {order.cliente}
                  <span className="block text-xs font-normal text-muted-foreground sm:hidden">
                    {order.produto}
                  </span>
                </TableCell>
                <TableCell className="hidden sm:table-cell">{order.produto}</TableCell>
                <TableCell className="text-right tabular-nums">{formatCurrency(order.valor)}</TableCell>
                <TableCell>
                  <OrderStatusBadge status={order.status} />
                </TableCell>
                <TableCell className="hidden text-muted-foreground md:table-cell">
                  {formatDateTime(order.data_criacao)}
                </TableCell>
                <TableCell className="hidden sm:table-cell">
                  <Button
                    variant="ghost"
                    size="sm"
                    aria-label={`Ver detalhes do pedido de ${order.cliente}`}
                    onClick={(e) => {
                      e.stopPropagation()
                      onSelect(order.id)
                    }}
                  >
                    Detalhes
                    <ChevronRight />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
      </TableBody>
    </Table>
  )
}
