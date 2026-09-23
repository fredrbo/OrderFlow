export type OrderStatus = 'Pendente' | 'Processando' | 'Finalizado'

export interface Order {
  id: string
  cliente: string
  produto: string
  valor: number
  status: OrderStatus
  data_criacao: string
}

export interface CreateOrderInput {
  cliente: string
  produto: string
  valor: number
}
