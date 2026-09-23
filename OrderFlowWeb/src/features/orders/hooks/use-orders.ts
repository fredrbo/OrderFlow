import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api/orders-api'
import type { CreateOrderInput, Order } from '../types'

const ACTIVE_REFETCH_INTERVAL_MS = 2_000
const IDLE_REFETCH_INTERVAL_MS = 15_000

const isActive = (order: Order) => order.status !== 'Finalizado'

export const getRefetchInterval = (orders: Order[] | undefined) =>
  orders?.some(isActive) ? ACTIVE_REFETCH_INTERVAL_MS : IDLE_REFETCH_INTERVAL_MS

const orderKeys = {
  all: ['orders'] as const,
  detail: (id: string) => ['orders', id] as const,
}

export function useOrders() {
  return useQuery({
    queryKey: orderKeys.all,
    queryFn: ordersApi.list,
    refetchInterval: (query) => getRefetchInterval(query.state.data),
  })
}

export function useOrder(id: string | null) {
  return useQuery({
    queryKey: orderKeys.detail(id ?? ''),
    queryFn: () => ordersApi.getById(id!),
    enabled: id !== null,
    refetchInterval: (query) => getRefetchInterval(query.state.data && [query.state.data]),
    retry: false,
  })
}

export function useCreateOrder() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateOrderInput) => ordersApi.create(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: orderKeys.all }),
  })
}
