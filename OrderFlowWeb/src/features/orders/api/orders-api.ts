import { http } from '@/api/http-client'
import type { CreateOrderInput, Order, OrderDetails } from '../types'

export const ordersApi = {
  list: () => http<Order[]>('/orders'),

  getById: (id: string) => http<OrderDetails>(`/orders/${id}`),

  create: (input: CreateOrderInput) =>
    http<Order>('/orders', { method: 'POST', body: JSON.stringify(input) }),
}
