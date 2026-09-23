import { http } from '@/api/http-client'
import type { CreateOrderInput, Order } from '../types'

export const ordersApi = {
  list: () => http<Order[]>('/orders'),

  getById: (id: string) => http<Order>(`/orders/${id}`),

  create: (input: CreateOrderInput) =>
    http<Order>('/orders', { method: 'POST', body: JSON.stringify(input) }),
}
