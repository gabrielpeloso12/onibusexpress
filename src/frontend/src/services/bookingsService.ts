import { apiRequest, ApiError } from './apiClient'
import type { BookingResponseDto, CreateBookingRequest } from '../types/api'

export async function createBooking(request: CreateBookingRequest): Promise<BookingResponseDto> {
  const booking = await apiRequest<BookingResponseDto>('/reservas', {
    method: 'POST',
    body: request,
  })

  if (!booking) throw new Error('Resposta de criação de reserva vazia.')
  return booking
}

export async function getBookingByCode(reservationCode: string): Promise<BookingResponseDto | null> {
  try {
    return await apiRequest<BookingResponseDto>(`/reservas/${encodeURIComponent(reservationCode)}`)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null
    throw error
  }
}

export async function cancelBooking(reservationCode: string): Promise<void> {
  await apiRequest<void>(`/reservas/${encodeURIComponent(reservationCode)}`, {
    method: 'DELETE',
  })
}
