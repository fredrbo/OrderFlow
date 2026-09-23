import { useState } from 'react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { AskOrdersCard } from '@/features/assistant/components/AskOrdersCard'
import { useOrderStatusWatcher } from '../hooks/use-order-status-watcher'
import { getRefetchInterval, useOrders } from '../hooks/use-orders'
import { CreateOrderDialog } from './CreateOrderDialog'
import { LiveUpdateIndicator } from './LiveUpdateIndicator'
import { OrderDetailsDialog } from './OrderDetailsDialog'
import { OrdersTable } from './OrdersTable'

export function OrdersPage() {
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const { data: orders, isLoading, dataUpdatedAt, error, refetch } = useOrders()
  const highlightedIds = useOrderStatusWatcher(orders)

  return (
    <div className="mx-auto grid w-full max-w-5xl gap-6 px-4 py-8 sm:px-6">
      <header className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">OrderFlow</h1>
          <p className="text-sm text-muted-foreground">Gerenciamento de pedidos</p>
        </div>
        <CreateOrderDialog />
      </header>

      {error && (
        <Alert variant="destructive">
          <AlertTitle>Erro ao carregar pedidos</AlertTitle>
          <AlertDescription className="flex flex-wrap items-center gap-3">
            {error.message}
            <Button variant="outline" size="sm" onClick={() => refetch()}>
              Tentar novamente
            </Button>
          </AlertDescription>
        </Alert>
      )}

      <AskOrdersCard />

      <Card>
        <CardHeader>
          <CardTitle>Pedidos</CardTitle>
          <CardDescription>Atualização automática a cada {getRefetchInterval(orders) / 1000}s</CardDescription>
          <CardAction>
            <LiveUpdateIndicator updatedAt={dataUpdatedAt} hasError={!!error} />
          </CardAction>
        </CardHeader>
        <CardContent>
          <OrdersTable
            orders={orders}
            isLoading={isLoading}
            highlightedIds={highlightedIds}
            onSelect={setSelectedId}
          />
        </CardContent>
      </Card>

      <OrderDetailsDialog orderId={selectedId} onClose={() => setSelectedId(null)} />
    </div>
  )
}
