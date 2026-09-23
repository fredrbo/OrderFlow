import { useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Loader2, Send, Sparkles } from 'lucide-react'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { assistantApi } from '../api/assistant-api'

const EXAMPLES = [
  'Quantos pedidos temos hoje?',
  'Qual o tempo médio para aprovar os pedidos?',
  'Quantos pedidos estão pendentes?',
  'Qual o valor total de pedidos finalizados este mês?',
]

export function AskOrdersCard() {
  const [question, setQuestion] = useState('')
  const ask = useMutation({ mutationFn: assistantApi.ask })

  const submit = (text: string) => {
    const trimmed = text.trim()
    if (!trimmed || ask.isPending) return
    setQuestion(trimmed)
    ask.mutate(trimmed)
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault()
    submit(question)
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Sparkles className="size-4 text-violet-500" aria-hidden />
          Pergunte sobre os pedidos
        </CardTitle>
        <CardDescription>Respostas geradas por IA a partir dos dados reais dos pedidos.</CardDescription>
      </CardHeader>
      <CardContent className="grid gap-4">
        <form onSubmit={onSubmit} className="flex gap-2">
          <Input
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
            placeholder="Ex.: Quantos pedidos estão pendentes?"
            maxLength={500}
            aria-label="Pergunta sobre os pedidos"
          />
          <Button type="submit" disabled={ask.isPending || !question.trim()}>
            {ask.isPending ? <Loader2 className="animate-spin" /> : <Send />}
            <span className="hidden sm:inline">Perguntar</span>
          </Button>
        </form>

        <div className="flex flex-wrap gap-2">
          {EXAMPLES.map((example) => (
            <Button
              key={example}
              type="button"
              variant="outline"
              size="sm"
              disabled={ask.isPending}
              onClick={() => submit(example)}
            >
              {example}
            </Button>
          ))}
        </div>

        {ask.isPending && (
          <p className="flex items-center gap-2 text-sm text-muted-foreground">
            <Loader2 className="size-4 animate-spin" aria-hidden />
            Consultando os pedidos…
          </p>
        )}

        {ask.isError && (
          <Alert variant="destructive">
            <AlertDescription>{ask.error.message}</AlertDescription>
          </Alert>
        )}

        {ask.isSuccess && (
          <div className="rounded-lg border border-violet-200 bg-violet-50 p-3 text-sm dark:border-violet-500/30 dark:bg-violet-500/10">
            <p className="mb-1 text-xs font-medium text-violet-700 dark:text-violet-300">{ask.variables}</p>
            <p className="whitespace-pre-line">{ask.data.resposta}</p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
