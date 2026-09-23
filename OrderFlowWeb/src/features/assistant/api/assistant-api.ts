import { http } from '@/api/http-client'

interface AskQuestionResponse {
  resposta: string
}

export const assistantApi = {
  ask: (pergunta: string) =>
    http<AskQuestionResponse>('/orders/ask', { method: 'POST', body: JSON.stringify({ pergunta }) }),
}
