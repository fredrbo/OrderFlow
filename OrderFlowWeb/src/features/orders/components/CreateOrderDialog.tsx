import { useState, type ReactNode } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2, Plus } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useCreateOrder } from '../hooks/use-orders'
import { createOrderSchema, type CreateOrderFormValues } from '../schemas/create-order-schema'

export function CreateOrderDialog() {
  const [open, setOpen] = useState(false)
  const createOrder = useCreateOrder()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateOrderFormValues>({
    resolver: zodResolver(createOrderSchema),
    defaultValues: { cliente: '', produto: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      const order = await createOrder.mutateAsync(values)
      toast.success('Pedido criado com sucesso', { description: `${order.produto} para ${order.cliente}` })
      setOpen(false)
      reset()
    } catch (error) {
      toast.error('Não foi possível criar o pedido', {
        description: error instanceof Error ? error.message : undefined,
      })
    }
  })

  const handleOpenChange = (next: boolean) => {
    setOpen(next)
    if (!next) reset()
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        <Button>
          <Plus />
          Novo pedido
        </Button>
      </DialogTrigger>
      <DialogContent>
        <form onSubmit={onSubmit} noValidate className="grid gap-4">
          <DialogHeader>
            <DialogTitle>Novo pedido</DialogTitle>
            <DialogDescription>O pedido será criado com status “Pendente”.</DialogDescription>
          </DialogHeader>

          <FormField id="cliente" label="Cliente" error={errors.cliente?.message}>
            <Input id="cliente" placeholder="Ex.: Maria Silva" autoFocus aria-invalid={!!errors.cliente} {...register('cliente')} />
          </FormField>

          <FormField id="produto" label="Produto" error={errors.produto?.message}>
            <Input id="produto" placeholder="Ex.: Notebook" aria-invalid={!!errors.produto} {...register('produto')} />
          </FormField>

          <FormField id="valor" label="Valor (R$)" error={errors.valor?.message}>
            <Input
              id="valor"
              type="number"
              inputMode="decimal"
              step="0.01"
              min="0"
              placeholder="0,00"
              aria-invalid={!!errors.valor}
              {...register('valor', { valueAsNumber: true })}
            />
          </FormField>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => handleOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={createOrder.isPending}>
              {createOrder.isPending && <Loader2 className="animate-spin" />}
              Criar pedido
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function FormField({
  id,
  label,
  error,
  children,
}: {
  id: string
  label: string
  error?: string
  children: ReactNode
}) {
  return (
    <div className="grid gap-2">
      <Label htmlFor={id}>{label}</Label>
      {children}
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
    </div>
  )
}
