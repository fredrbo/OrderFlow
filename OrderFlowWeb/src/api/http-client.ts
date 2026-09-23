const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5099'

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function toError(response: Response): Promise<Error> {
  const fallback = `Erro ${response.status} ao chamar a API.`
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null

  if (!problem) return new Error(fallback)

  const validationMessages = problem.errors ? Object.values(problem.errors).flat() : []
  return new Error(validationMessages[0] ?? problem.detail ?? problem.title ?? fallback)
}

export async function http<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response

  try {
    response = await fetch(`${API_URL}${path}`, {
      ...init,
      headers: { 'Content-Type': 'application/json', Accept: 'application/json', ...init?.headers },
    })
  } catch {
    throw new Error('Não foi possível conectar à API. Verifique se ela está rodando.')
  }

  if (!response.ok) throw await toError(response)

  return (await response.json()) as T
}
