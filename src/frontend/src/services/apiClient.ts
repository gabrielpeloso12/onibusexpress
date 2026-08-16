import type { ProblemDetails } from '../types/api'

const API_BASE_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5083'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'DELETE'
  body?: unknown
  query?: Record<string, string | undefined>
}

function buildUrl(path: string, query?: Record<string, string | undefined>): string {
  const url = new URL(path, API_BASE_URL)

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value) url.searchParams.set(key, value)
    }
  }

  return url.toString()
}

/**
 * Wrapper fino sobre `fetch` para conversar com a OniBus Express API: monta a URL a partir de
 * `VITE_API_URL`, serializa/desserializa JSON e traduz respostas de erro (ProblemDetails,
 * gerado pelo middleware de exceções do backend) em `ApiError` com uma mensagem amigável.
 */
export async function apiRequest<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse | null> {
  const { method = 'GET', body, query } = options

  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const response = await fetch(buildUrl(path, query), {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  if (response.status === 204) return null

  const isJson = response.headers.get('content-type')?.includes('json') ?? false
  const payload = isJson ? await response.json() : undefined

  if (!response.ok) {
    const problem = payload as ProblemDetails | undefined
    throw new ApiError(response.status, problem?.detail ?? defaultErrorMessage(response.status))
  }

  return payload as TResponse
}

function defaultErrorMessage(status: number): string {
  switch (status) {
    case 400:
      return 'Requisição inválida. Confira os dados informados.'
    case 404:
      return 'Recurso não encontrado.'
    case 409:
      return 'Não foi possível concluir a operação devido a um conflito.'
    default:
      return 'Ocorreu um erro inesperado. Tente novamente em instantes.'
  }
}
