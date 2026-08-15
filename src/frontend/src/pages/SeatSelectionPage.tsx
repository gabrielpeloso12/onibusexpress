import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorMessage } from '../components/ErrorMessage'
import { SeatMap } from '../components/SeatMap'
import { Stepper } from '../components/Stepper'
import { PURCHASE_STEPS } from '../constants/steps'
import { useBooking } from '../context/BookingContext'
import { getTripById } from '../services/tripsService'
import { formatCurrency, formatDateTime, formatDuration } from '../utils/format'
import type { TripDetailsDto } from '../types/api'

export function SeatSelectionPage() {
  const { tripId } = useParams<{ tripId: string }>()
  const navigate = useNavigate()
  const { selectedTrip, setSelectedTrip, selectedSeat, setSelectedSeat } = useBooking()

  const [trip, setTrip] = useState<TripDetailsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!tripId) return

    if (selectedTrip?.id === tripId) {
      setTrip(selectedTrip)
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    setSelectedSeat(null)
    getTripById(tripId)
      .then((result) => {
        if (!result) {
          setError('Viagem não encontrada. Ela pode ter sido removida.')
          return
        }
        setTrip(result)
        setSelectedTrip(result)
      })
      .catch(() => setError('Não foi possível carregar os assentos dessa viagem.'))
      .finally(() => setLoading(false))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tripId])

  function handleSeatSelect(seatNumber: number) {
    setSelectedSeat(seatNumber)
  }

  function handleContinue() {
    navigate('/checkout')
  }

  if (loading) {
    return (
      <div className="container">
        <LoadingSpinner label="Carregando assentos..." />
      </div>
    )
  }

  if (error || !trip) {
    return (
      <div className="container">
        <ErrorMessage message={error ?? 'Viagem não encontrada.'} />
        <button type="button" className="btn btn-secondary" onClick={() => navigate('/')}>
          Voltar para a busca
        </button>
      </div>
    )
  }

  return (
    <div className="container">
      <Stepper steps={PURCHASE_STEPS} currentStep={1} />

      <h1>Selecione seu assento</h1>

      <div className="card" style={{ marginBottom: '1.5rem' }}>
        <strong>
          {trip.origin} → {trip.destination}
        </strong>
        <p className="muted" style={{ marginTop: '0.4rem', marginBottom: 0 }}>
          {formatDateTime(trip.departureDateTime)} · duração aproximada de {formatDuration(trip.estimatedDuration)} ·{' '}
          {formatCurrency(trip.basePrice)} por assento
        </p>
      </div>

      <SeatMap seats={trip.seats} selectedSeat={selectedSeat} onSelect={handleSeatSelect} />

      <div style={{ marginTop: '1.5rem', display: 'flex', gap: '0.75rem' }}>
        <button type="button" className="btn btn-secondary" onClick={() => navigate('/')}>
          Voltar
        </button>
        <button type="button" className="btn btn-primary" disabled={selectedSeat === null} onClick={handleContinue}>
          {selectedSeat === null ? 'Selecione um assento' : `Continuar com o assento ${selectedSeat}`}
        </button>
      </div>
    </div>
  )
}
