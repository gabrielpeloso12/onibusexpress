import { describe, expect, it, vi, beforeEach } from 'vitest'
import { cancelBooking, createBooking, getBookingByCode } from '../../services/bookingsService'
import type { BookingResponseDto } from '../../types/api'

const booking: BookingResponseDto = {
  reservationCode: 'ABC-12345',
  status: 'Confirmed',
  tripId: 'trip-1',
  origin: 'São Paulo',
  destination: 'Rio de Janeiro',
  departureDateTime: '2026-08-20T08:00:00',
  basePrice: 120,
  seatNumber: 5,
  passengerName: 'Maria da Silva',
  passengerCpf: '529.982.247-25',
  passengerEmail: 'maria@example.com',
  createdAtUtc: new Date().toISOString(),
  cancelledAtUtc: null,
}

function jsonResponse(status: number, body: unknown) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'content-type': 'application/json' },
  })
}

beforeEach(() => {
  vi.restoreAllMocks()
})

describe('bookingsService', () => {
  it('createBooking envia POST /reservas sem autenticação e retorna a reserva criada', async () => {
    const fetchMock = vi.fn((input: RequestInfo | URL, _init?: RequestInit) => {
      const url = input.toString()
      if (url.includes('/reservas')) return Promise.resolve(jsonResponse(201, booking))
      throw new Error(`unexpected url: ${url}`)
    })
    vi.stubGlobal('fetch', fetchMock)

    const result = await createBooking({
      passengerName: 'Maria da Silva',
      passengerCpf: '52998224725',
      passengerEmail: 'maria@example.com',
      passengerBirthDate: '1990-01-01',
      tripId: 'trip-1',
      seatNumber: 5,
    })

    expect(result.reservationCode).toBe('ABC-12345')

    const [, init] = fetchMock.mock.calls[0]
    expect(init?.method).toBe('POST')
    expect((init?.headers as Record<string, string> | undefined)?.Authorization).toBeUndefined()
  })

  it('cancelBooking envia DELETE /reservas/{codigo} sem autenticação', async () => {
    const fetchMock = vi.fn((_input: RequestInfo | URL, _init?: RequestInit) =>
      Promise.resolve(new Response(null, { status: 204 })),
    )
    vi.stubGlobal('fetch', fetchMock)

    await cancelBooking('ABC-12345')

    const [url, init] = fetchMock.mock.calls[0]
    expect(url.toString()).toContain('/reservas/ABC-12345')
    expect(init?.method).toBe('DELETE')
    expect((init?.headers as Record<string, string> | undefined)?.Authorization).toBeUndefined()
  })

  it('getBookingByCode retorna null quando a API responde 404', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.resolve(jsonResponse(404, { detail: 'não encontrada' }))),
    )

    const result = await getBookingByCode('ZZZ-99999')

    expect(result).toBeNull()
  })
})
