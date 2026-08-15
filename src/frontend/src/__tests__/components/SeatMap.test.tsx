import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SeatMap } from '../../components/SeatMap'
import type { SeatDto } from '../../types/api'

const seats: SeatDto[] = [
  { seatNumber: 1, isOccupied: false },
  { seatNumber: 2, isOccupied: true },
  { seatNumber: 3, isOccupied: false },
]

describe('SeatMap', () => {
  it('desabilita assentos ocupados e mantém os livres clicáveis', () => {
    render(<SeatMap seats={seats} selectedSeat={null} onSelect={vi.fn()} />)

    expect(screen.getByRole('button', { name: /assento 2, ocupado/i })).toBeDisabled()
    expect(screen.getByRole('button', { name: /assento 1, livre/i })).toBeEnabled()
  })

  it('chama onSelect com o número do assento ao clicar em um assento livre', async () => {
    const user = userEvent.setup()
    const onSelect = vi.fn()
    render(<SeatMap seats={seats} selectedSeat={null} onSelect={onSelect} />)

    await user.click(screen.getByRole('button', { name: /assento 3, livre/i }))

    expect(onSelect).toHaveBeenCalledWith(3)
  })

  it('marca o assento selecionado com aria-pressed', () => {
    render(<SeatMap seats={seats} selectedSeat={1} onSelect={vi.fn()} />)

    expect(screen.getByRole('button', { name: /assento 1, selecionado/i })).toHaveAttribute('aria-pressed', 'true')
  })
})
