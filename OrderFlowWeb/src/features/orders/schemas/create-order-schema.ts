import { z } from 'zod'

export const createOrderSchema = z.object({
  cliente: z
    .string()
    .trim()
    .min(1, 'O cliente é obrigatório.')
    .max(200, 'O cliente deve ter no máximo 200 caracteres.'),
  produto: z
    .string()
    .trim()
    .min(1, 'O produto é obrigatório.')
    .max(200, 'O produto deve ter no máximo 200 caracteres.'),
  valor: z
    .number({ error: 'Informe um valor válido.' })
    .positive('O valor deve ser maior que zero.'),
})

export type CreateOrderFormValues = z.infer<typeof createOrderSchema>
