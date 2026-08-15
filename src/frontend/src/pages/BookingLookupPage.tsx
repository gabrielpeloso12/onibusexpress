import { useState } from 'react'
import type { FormEvent } from 'react'
import { ErrorMessage } from '../components/ErrorMessage'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ApiError } from '../services/apiClient'
import { cancelBooking, getBookingByCode } from '../services/bookingsService'
import { formatCurrency, formatDateTime } from '../utils/format'
import type { BookingResponseDto } from '../types/api'

export function BookingLookupPage() {
  const [code, setCode] = useState('')
  const [booking, setBooking] = useState<BookingResponseDto | null>(null)
  const [searched, setSearched] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [cancelling, setCancelling] = useState(false)
  const [cancelError, setCancelError] = useState<string | null>(null)
  const [cancelled, setCancelled] = useState(false)

  async function handleSearch(event: FormEvent) {
    event.preventDefault()
    if (!code.trim()) return

    setLoading(true)
    setError(null)
    setCancelError(null)
    setCancelled(false)

    try {
      const result = await getBookingByCode(code.trim())
      setBooking(result)
      setSearched(true)
      if (!result) setError('Nenhuma reserva foi encontrada com esse código.')
    } catch {
      setError('Não foi possível consultar a reserva agora. Tente novamente.')
      setBooking(null)
    } finally {
      setLoading(false)
    }
  }

  async function handleCancel() {
    if (!booking) return

    setCancelling(true)
    setCancelError(null)

    try {
      await cancelBooking(booking.reservationCode)
      setCancelled(true)
      setBooking({ ...booking, status: 'Cancelled', cancelledAtUtc: new Date().toISOString() })
    } catch (err) {
      setCancelError(
        err instanceof ApiError
          ? err.message
          : 'Não foi possível cancelar a reserva agora. Tente novamente.',
      )
    } finally {
      setCancelling(false)
    }
  }

  return (
    <div className="container">
      <h1>Consultar reserva</h1>
      <p>Digite o código recebido na confirmação da compra (ex.: ABC-12345).</p>

      <form className="card" onSubmit={handleSearch} style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-end' }}>
        <div className="field" style={{ flex: 1, marginBottom: 0 }}>
          <label htmlFor="code">Código da reserva</label>
          <input
            id="code"
            value={code}
            onChange={(e) => setCode(e.target.value.toUpperCase())}
            placeholder="ABC-12345"
          />
        </div>
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Consultando...' : 'Consultar'}
        </button>
      </form>

      <section style={{ marginTop: '1.5rem' }}>
        {loading && <LoadingSpinner label="Consultando reserva..." />}
        {!loading && error && <ErrorMessage message={error} />}

        {!loading && searched && booking && (
          <div className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '1rem' }}>
              <div>
                <strong>{booking.reservationCode}</strong>
                <p style={{ margin: '0.4rem 0 0' }}>
                  {booking.origin} → {booking.destination}
                  <br />
                  {formatDateTime(booking.departureDateTime)} · assento {booking.seatNumber} ·{' '}
                  {formatCurrency(booking.basePrice)}
                  <br />
                  Passageiro: {booking.passengerName} ({booking.passengerCpf})
                </p>
              </div>
              <span
                className="alert"
                style={{
                  margin: 0,
                  padding: '0.35rem 0.75rem',
                  background: booking.status === 'Confirmed' ? 'var(--color-primary-light)' : 'var(--color-danger-light)',
                  color: booking.status === 'Confirmed' ? 'var(--color-primary-dark)' : 'var(--color-danger)',
                }}
              >
                {booking.status === 'Confirmed' ? 'Confirmada' : 'Cancelada'}
              </span>
            </div>

            {cancelError && (
              <div style={{ marginTop: '1rem' }}>
                <ErrorMessage message={cancelError} />
              </div>
            )}

            {cancelled && (
              <div className="alert alert-info" style={{ marginTop: '1rem' }}>
                Reserva cancelada com sucesso.
              </div>
            )}

            {booking.status === 'Confirmed' && !cancelled && (
              <div style={{ marginTop: '1rem' }}>
                <button type="button" className="btn btn-danger" disabled={cancelling} onClick={handleCancel}>
                  {cancelling ? 'Cancelando...' : 'Cancelar reserva'}
                </button>
                <p className="muted" style={{ marginTop: '0.5rem', marginBottom: 0, fontSize: '0.85rem' }}>
                  Cancelamentos só são permitidos até 2 horas antes da partida.
                </p>
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  )
}
