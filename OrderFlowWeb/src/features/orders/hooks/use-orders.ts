import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api/orders-api'
import type { CreateOrderInput } from '../types'

export const ORDERS_REFETCH_INTERVAL_MS = 15_000

const orderKeys = {
  all: ['orders'] as const,
  detail: (id: string) => ['orders', id] as const,
}

export function useOrders() {
  return useQuery({
    queryKey: orderKeys.all,
    queryFn: ordersApi.list,
    refetchInterval: ORDERS_REFETCH_INTERVAL_MS,
  })
}

export function useOrder(id: string | null) {
  return useQuery({
    queryKey: orderKeys.detail(id ?? ''),
    queryFn: () => ordersApi.getById(id!),
    enabled: id !== null,
    refetchInterval: ORDERS_REFETCH_INTERVAL_MS,
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
