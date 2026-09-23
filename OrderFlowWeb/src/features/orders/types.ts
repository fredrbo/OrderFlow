export type OrderStatus = 'Pendente' | 'Processando' | 'Finalizado'

export interface Order {
  id: string
  cliente: string
  produto: string
  valor: number
  status: OrderStatus
  data_criacao: string
  data_finalizacao: string | null
}

export interface OrderStatusHistoryEntry {
  status: OrderStatus
  data_alteracao: string
}

export interface OrderDetails extends Order {
  historico: OrderStatusHistoryEntry[]
}

export interface CreateOrderInput {
  cliente: string
  produto: string
  valor: number
}
