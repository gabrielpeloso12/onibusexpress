import { apiRequest, ApiError } from './apiClient'
import type { TripDetailsDto, TripSummaryDto } from '../types/api'

export interface TripSearchParams {
  origem?: string
  destino?: string
  data?: string
  [key: string]: string | undefined
}

export async function searchTrips(params: TripSearchParams): Promise<TripSummaryDto[]> {
  const trips = await apiRequest<TripSummaryDto[]>('/viagens', { query: params })
  return trips ?? []
}

export async function getTripById(tripId: string): Promise<TripDetailsDto | null> {
  try {
    return await apiRequest<TripDetailsDto>(`/viagens/${tripId}`)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}
