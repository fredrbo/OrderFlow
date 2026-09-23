import type { ReactNode } from 'react'
import { AlertTriangle, Loader2, WifiOff } from 'lucide-react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCurrency, formatDateTime, formatTime } from '@/lib/format'
import { useOrder } from '../hooks/use-orders'
import { OrderStatusBadge } from './OrderStatusBadge'

interface OrderDetailsDialogProps {
  orderId: string | null
  onClose: () => void
}

export function OrderDetailsDialog({ orderId, onClose }: OrderDetailsDialogProps) {
  const { data: order, isLoading, error, isFetching, refetch, dataUpdatedAt } = useOrder(orderId)

  const retryButton = (
    <div className="mt-2">
      <Button variant="outline" size="sm" disabled={isFetching} onClick={() => refetch()}>
        {isFetching && <Loader2 className="animate-spin" />}
        Tentar novamente
      </Button>
    </div>
  )

  return (
    <Dialog open={orderId !== null} onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Detalhes do pedido</DialogTitle>
          <DialogDescription className="font-mono text-xs break-all">{orderId}</DialogDescription>
        </DialogHeader>

        {isLoading && (
          <div className="grid gap-3">
            {Array.from({ length: 5 }, (_, i) => (
              <Skeleton key={i} className="h-5 w-full" />
            ))}
          </div>
        )}

        {error && !order && (
          <Alert variant="destructive">
            <WifiOff />
            <AlertTitle>Não foi possível carregar o pedido</AlertTitle>
            <AlertDescription>
              {error.message}
              {retryButton}
            </AlertDescription>
          </Alert>
        )}

        {error && order && (
          <Alert variant="warning">
            <AlertTriangle />
            <AlertTitle>Não foi possível atualizar</AlertTitle>
            <AlertDescription>
              Exibindo os dados carregados às {formatTime(dataUpdatedAt)}, que podem estar desatualizados.
              {retryButton}
            </AlertDescription>
          </Alert>
        )}

        {order && (
          <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-3">
            <Detail label="Cliente">{order.cliente}</Detail>
            <Detail label="Produto">{order.produto}</Detail>
            <Detail label="Valor">
              <span className="tabular-nums">{formatCurrency(order.valor)}</span>
            </Detail>
            <Detail label="Status">
              <OrderStatusBadge status={order.status} />
            </Detail>
            <Detail label="Criado em">{formatDateTime(order.data_criacao)}</Detail>
          </dl>
        )}
      </DialogContent>
    </Dialog>
  )
}

function Detail({ label, children }: { label: string; children: ReactNode }) {
  return (
    <>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="font-medium">{children}</dd>
    </>
  )
}
