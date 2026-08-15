import { createContext, useContext, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import type { TripDetailsDto } from '../types/api'

interface BookingContextValue {
  /** Viagem escolhida na Tela 1, usada nas Telas 2 e 3. */
  selectedTrip: TripDetailsDto | null
  /** Assento escolhido na Tela 2, usado na Tela 3. */
  selectedSeat: number | null
  setSelectedTrip: (trip: TripDetailsDto | null) => void
  setSelectedSeat: (seat: number | null) => void
  /** Limpa a seleção; chamado ao voltar para a busca ou após concluir/abandonar uma compra. */
  reset: () => void
}

const BookingContext = createContext<BookingContextValue | undefined>(undefined)

/**
 * Guarda o estado do fluxo de compra (viagem e assento escolhidos) compartilhado entre as
 * Telas 1→2→3. Usamos a Context API nativa do React em vez de Redux/Zustand porque o estado
 * é pequeno, local a um único fluxo linear e não precisa ser lido fora da árvore de componentes
 * (sem persistência entre sessões, sem devtools de time-travel) — ver README > FRONTEND.
 */
export function BookingProvider({ children }: { children: ReactNode }) {
  const [selectedTrip, setSelectedTrip] = useState<TripDetailsDto | null>(null)
  const [selectedSeat, setSelectedSeat] = useState<number | null>(null)

  const value = useMemo<BookingContextValue>(
    () => ({
      selectedTrip,
      selectedSeat,
      setSelectedTrip,
      setSelectedSeat,
      reset: () => {
        setSelectedTrip(null)
        setSelectedSeat(null)
      },
    }),
    [selectedTrip, selectedSeat],
  )

  return <BookingContext.Provider value={value}>{children}</BookingContext.Provider>
}

export function useBooking(): BookingContextValue {
  const context = useContext(BookingContext)
  if (!context) throw new Error('useBooking deve ser usado dentro de um BookingProvider.')
  return context
}
