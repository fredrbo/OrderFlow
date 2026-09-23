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

export interface CreateOrderInput {
  cliente: string
  produto: string
  valor: number
}
