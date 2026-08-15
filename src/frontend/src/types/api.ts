/**
 * Tipos espelhando os DTOs expostos pela OniBus Express API.
 * Mantidos manualmente (sem geração automática) por serem poucos e estáveis;
 * ver README > FRONTEND para a decisão.
 */

export interface RouteDto {
  id: string
  origin: string
  destination: string
  /** Formato "hh:mm:ss" (TimeSpan serializado pelo backend). */
  estimatedDuration: string
}

export interface TripSummaryDto {
  id: string
  routeId: string
  origin: string
  destination: string
  /** ISO 8601 em UTC (sufixo "Z"); exibido já convertido para o fuso do navegador via `formatDateTime`. */
  departureDateTime: string
  basePrice: number
  availableSeats: number
  totalSeats: number
}

export interface SeatDto {
  seatNumber: number
  isOccupied: boolean
}

export interface TripDetailsDto {
  id: string
  routeId: string
  origin: string
  destination: string
  estimatedDuration: string
  departureDateTime: string
  basePrice: number
  totalSeats: number
  seats: SeatDto[]
}

export type BookingStatus = 'Confirmed' | 'Cancelled'

export interface CreateBookingRequest {
  passengerName: string
  passengerCpf: string
  passengerEmail: string
  /** Formato "yyyy-MM-dd". */
  passengerBirthDate: string
  tripId: string
  seatNumber: number
}

export interface BookingResponseDto {
  reservationCode: string
  status: BookingStatus
  tripId: string
  origin: string
  destination: string
  departureDateTime: string
  basePrice: number
  seatNumber: number
  passengerName: string
  passengerCpf: string
  passengerEmail: string
  createdAtUtc: string
  cancelledAtUtc: string | null
}

/** Corpo padrão do ProblemDetails retornado pelo middleware de exceções da API em erros de negócio. */
export interface ProblemDetails {
  title?: string
  status?: number
  detail?: string
  instance?: string
}
