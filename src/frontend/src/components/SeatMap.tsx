import type { SeatDto } from '../types/api'
import styles from './SeatMap.module.css'

interface SeatMapProps {
  seats: SeatDto[]
  selectedSeat: number | null
  onSelect: (seatNumber: number) => void
}

function seatState(seat: SeatDto, selectedSeat: number | null): 'occupied' | 'selected' | 'free' {
  if (seat.isOccupied) return 'occupied'
  if (seat.seatNumber === selectedSeat) return 'selected'
  return 'free'
}

const STATE_LABEL: Record<ReturnType<typeof seatState>, string> = {
  free: 'livre',
  occupied: 'ocupado',
  selected: 'selecionado',
}

export function SeatMap({ seats, selectedSeat, onSelect }: SeatMapProps) {
  return (
    <div>
      <ul className={styles.legend}>
        <li>
          <span className={`${styles.swatch} ${styles.free}`} aria-hidden="true" /> Livre
        </li>
        <li>
          <span className={`${styles.swatch} ${styles.occupied}`} aria-hidden="true" /> Ocupado
        </li>
        <li>
          <span className={`${styles.swatch} ${styles.selected}`} aria-hidden="true" /> Selecionado
        </li>
      </ul>

      <div className={styles.grid} role="group" aria-label="Mapa de assentos">
        {seats.map((seat) => {
          const state = seatState(seat, selectedSeat)
          return (
            <button
              key={seat.seatNumber}
              type="button"
              className={`${styles.seat} ${styles[state]}`}
              disabled={seat.isOccupied}
              aria-pressed={state === 'selected'}
              aria-label={`Assento ${seat.seatNumber}, ${STATE_LABEL[state]}`}
              onClick={() => onSelect(seat.seatNumber)}
            >
              {seat.seatNumber}
            </button>
          )
        })}
      </div>
    </div>
  )
}
