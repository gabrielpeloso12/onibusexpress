import { apiRequest } from './apiClient'
import type { RouteDto } from '../types/api'

export async function getRoutes(): Promise<RouteDto[]> {
  const routes = await apiRequest<RouteDto[]>('/rotas')
  return routes ?? []
}
