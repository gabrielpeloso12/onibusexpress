import type { TripSummaryDto } from '../types/api'
import { formatCurrency, formatDateTime } from '../utils/format'
import styles from './TripCard.module.css'

interface TripCardProps {
  trip: TripSummaryDto
  onSelect: (trip: TripSummaryDto) => void
}

export function TripCard({ trip, onSelect }: TripCardProps) {
  const soldOut = trip.availableSeats === 0

  return (
    <li className={`card ${styles.card}`}>
      <div className={styles.route}>
        <strong>{trip.origin}</strong>
        <span aria-hidden="true"> → </span>
        <strong>{trip.destination}</strong>
      </div>

      <div className={styles.details}>
        <span>{formatDateTime(trip.departureDateTime)}</span>
        <span className={soldOut ? styles.soldOut : undefined}>
          {soldOut ? 'Sem vagas' : `${trip.availableSeats} de ${trip.totalSeats} assentos livres`}
        </span>
      </div>

      <div className={styles.footer}>
        <span className={styles.price}>{formatCurrency(trip.basePrice)}</span>
        <button type="button" className="btn btn-primary" disabled={soldOut} onClick={() => onSelect(trip)}>
          Selecionar assento
        </button>
      </div>
    </li>
  )
}
