import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorMessage } from '../components/ErrorMessage'
import { TripCard } from '../components/TripCard'
import { Stepper } from '../components/Stepper'
import { PURCHASE_STEPS } from '../constants/steps'
import { getRoutes } from '../services/routesService'
import { searchTrips } from '../services/tripsService'
import type { RouteDto, TripSummaryDto } from '../types/api'

export function SearchPage() {
  const navigate = useNavigate()

  const [routes, setRoutes] = useState<RouteDto[]>([])
  const [origin, setOrigin] = useState('')
  const [destination, setDestination] = useState('')
  const [date, setDate] = useState('')

  const [trips, setTrips] = useState<TripSummaryDto[] | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getRoutes()
      .then(setRoutes)
      .catch(() => setRoutes([]))
  }, [])

  const originOptions = useMemo(() => Array.from(new Set(routes.map((r) => r.origin))).sort(), [routes])
  const destinationOptions = useMemo(() => Array.from(new Set(routes.map((r) => r.destination))).sort(), [routes])

  async function handleSearch(event: FormEvent) {
    event.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const results = await searchTrips({
        origem: origin.trim() || undefined,
        destino: destination.trim() || undefined,
        data: date || undefined,
      })
      setTrips(results)
    } catch {
      setError('Não foi possível buscar as viagens agora. Tente novamente em instantes.')
      setTrips(null)
    } finally {
      setLoading(false)
    }
  }

  function handleSelectTrip(trip: TripSummaryDto) {
    navigate(`/viagens/${trip.id}/assentos`)
  }

  return (
    <div className="container">
      <Stepper steps={PURCHASE_STEPS} currentStep={0} />

      <h1>Busca de passagens</h1>
      <p>Encontre a viagem ideal informando origem, destino e data.</p>

      <form className="card" onSubmit={handleSearch}>
        <div className="field">
          <label htmlFor="origin">Origem</label>
          <input
            id="origin"
            list="origin-options"
            value={origin}
            onChange={(e) => setOrigin(e.target.value)}
            placeholder="Ex.: São Paulo"
          />
          <datalist id="origin-options">
            {originOptions.map((value) => (
              <option key={value} value={value} />
            ))}
          </datalist>
        </div>

        <div className="field">
          <label htmlFor="destination">Destino</label>
          <input
            id="destination"
            list="destination-options"
            value={destination}
            onChange={(e) => setDestination(e.target.value)}
            placeholder="Ex.: Rio de Janeiro"
          />
          <datalist id="destination-options">
            {destinationOptions.map((value) => (
              <option key={value} value={value} />
            ))}
          </datalist>
        </div>

        <div className="field">
          <label htmlFor="date">Data de ida</label>
          <input id="date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        </div>

        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Buscando...' : 'Buscar'}
        </button>
      </form>

      <section style={{ marginTop: '2rem' }}>
        {loading && <LoadingSpinner label="Buscando viagens..." />}
        {error && <ErrorMessage message={error} />}

        {!loading && trips !== null && trips.length === 0 && (
          <p className="muted">Nenhuma viagem encontrada para os filtros informados. Tente ajustar a busca.</p>
        )}

        {!loading && trips !== null && trips.length > 0 && (
          <ul style={{ listStyle: 'none', padding: 0, display: 'grid', gap: '1rem' }}>
            {trips.map((trip) => (
              <TripCard key={trip.id} trip={trip} onSelect={handleSelectTrip} />
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
