import { describe, expect, it, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { SearchPage } from '../../pages/SearchPage'
import * as routesService from '../../services/routesService'
import * as tripsService from '../../services/tripsService'
import type { TripSummaryDto } from '../../types/api'

vi.mock('../../services/routesService')
vi.mock('../../services/tripsService')

const trip: TripSummaryDto = {
  id: 'trip-1',
  routeId: 'route-1',
  origin: 'São Paulo',
  destination: 'Rio de Janeiro',
  departureDateTime: '2026-08-20T08:00:00',
  basePrice: 120,
  availableSeats: 39,
  totalSeats: 40,
}

beforeEach(() => {
  vi.mocked(routesService.getRoutes).mockResolvedValue([])
})

describe('SearchPage', () => {
  it('mostra estado de carregamento e depois a lista de viagens encontradas', async () => {
    vi.mocked(tripsService.searchTrips).mockResolvedValue([trip])
    const user = userEvent.setup()

    render(
      <MemoryRouter>
        <SearchPage />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /buscar/i }))

    await waitFor(() => {
      expect(screen.getByText(/São Paulo/)).toBeInTheDocument()
      expect(screen.getByText(/Rio de Janeiro/)).toBeInTheDocument()
    })
  })

  it('mostra mensagem quando a busca não encontra viagens', async () => {
    vi.mocked(tripsService.searchTrips).mockResolvedValue([])
    const user = userEvent.setup()

    render(
      <MemoryRouter>
        <SearchPage />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /buscar/i }))

    expect(await screen.findByText(/nenhuma viagem encontrada/i)).toBeInTheDocument()
  })

  it('mostra mensagem de erro quando a busca falha', async () => {
    vi.mocked(tripsService.searchTrips).mockRejectedValue(new Error('network error'))
    const user = userEvent.setup()

    render(
      <MemoryRouter>
        <SearchPage />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /buscar/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/não foi possível buscar/i)
  })
})
