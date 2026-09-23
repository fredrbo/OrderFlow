import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from '@/components/ui/sonner'
import { OrdersPage } from '@/features/orders/components/OrdersPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: true,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <main className="min-h-svh bg-muted/40">
        <OrdersPage />
      </main>
      <Toaster richColors position="bottom-right" />
    </QueryClientProvider>
  )
}
