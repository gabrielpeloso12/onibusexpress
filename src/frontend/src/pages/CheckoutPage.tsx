import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ErrorMessage } from '../components/ErrorMessage'
import { Stepper } from '../components/Stepper'
import { PURCHASE_STEPS } from '../constants/steps'
import { useBooking } from '../context/BookingContext'
import { createBooking } from '../services/bookingsService'
import { ApiError } from '../services/apiClient'
import { formatCpf, isValidCpf } from '../utils/cpf'
import { formatCurrency, formatDateTime } from '../utils/format'
import type { BookingResponseDto } from '../types/api'

interface FormState {
  name: string
  cpf: string
  email: string
  birthDate: string
}

interface FormErrors {
  name?: string
  cpf?: string
  email?: string
  birthDate?: string
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate(form: FormState): FormErrors {
  const errors: FormErrors = {}

  if (form.name.trim().length < 3) errors.name = 'Informe o nome completo.'
  if (!isValidCpf(form.cpf)) errors.cpf = 'CPF inválido. Confira os números digitados.'
  if (!EMAIL_PATTERN.test(form.email)) errors.email = 'Informe um e-mail válido.'
  if (!form.birthDate) errors.birthDate = 'Informe a data de nascimento.'
  else if (new Date(form.birthDate) >= new Date()) errors.birthDate = 'A data de nascimento deve estar no passado.'

  return errors
}

export function CheckoutPage() {
  const navigate = useNavigate()
  const { selectedTrip, selectedSeat, reset } = useBooking()

  const [form, setForm] = useState<FormState>({ name: '', cpf: '', email: '', birthDate: '' })
  const [errors, setErrors] = useState<FormErrors>({})
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [booking, setBooking] = useState<BookingResponseDto | null>(null)

  useEffect(() => {
    if (!selectedTrip || selectedSeat === null) navigate('/', { replace: true })
  }, [selectedTrip, selectedSeat, navigate])

  if (!selectedTrip || selectedSeat === null) return null

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()

    const validationErrors = validate(form)
    setErrors(validationErrors)
    if (Object.keys(validationErrors).length > 0) return

    setSubmitting(true)
    setSubmitError(null)

    try {
      const result = await createBooking({
        passengerName: form.name.trim(),
        passengerCpf: form.cpf,
        passengerEmail: form.email.trim(),
        passengerBirthDate: form.birthDate,
        tripId: selectedTrip!.id,
        seatNumber: selectedSeat!,
      })
      setBooking(result)
    } catch (error) {
      setSubmitError(error instanceof ApiError ? error.message : 'Não foi possível concluir a reserva. Tente novamente.')
    } finally {
      setSubmitting(false)
    }
  }

  function handleNewSearch() {
    reset()
    navigate('/')
  }

  if (booking) {
    return (
      <div className="container">
        <div className="card" style={{ textAlign: 'center' }}>
          <h1>Reserva confirmada! 🎉</h1>
          <p>Guarde o código abaixo para consultar ou cancelar sua passagem quando precisar.</p>
          <p
            style={{
              fontSize: '2rem',
              fontWeight: 700,
              letterSpacing: '0.05em',
              color: 'var(--color-primary-dark)',
              margin: '1rem 0',
            }}
          >
            {booking.reservationCode}
          </p>
          <p className="muted">
            {booking.origin} → {booking.destination} · {formatDateTime(booking.departureDateTime)} · assento{' '}
            {booking.seatNumber} · {formatCurrency(booking.basePrice)}
          </p>
          <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'center', marginTop: '1.5rem' }}>
            <button type="button" className="btn btn-secondary" onClick={() => navigate('/consulta')}>
              Consultar esta reserva
            </button>
            <button type="button" className="btn btn-primary" onClick={handleNewSearch}>
              Fazer nova busca
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="container">
      <Stepper steps={PURCHASE_STEPS} currentStep={2} />

      <h1>Confirme seus dados</h1>

      <div className="card" style={{ marginBottom: '1.5rem' }}>
        <strong>Resumo da compra</strong>
        <p style={{ marginTop: '0.5rem', marginBottom: 0 }}>
          {selectedTrip.origin} → {selectedTrip.destination}
          <br />
          {formatDateTime(selectedTrip.departureDateTime)} · assento {selectedSeat}
          <br />
          <span style={{ fontWeight: 700, color: 'var(--color-primary-dark)' }}>
            {formatCurrency(selectedTrip.basePrice)}
          </span>
        </p>
      </div>

      <form className="card" onSubmit={handleSubmit} noValidate>
        {submitError && <ErrorMessage message={submitError} />}

        <div className="field">
          <label htmlFor="name">Nome completo</label>
          <input
            id="name"
            value={form.name}
            onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            aria-invalid={Boolean(errors.name)}
          />
          {errors.name && <span className="field-error">{errors.name}</span>}
        </div>

        <div className="field">
          <label htmlFor="cpf">CPF</label>
          <input
            id="cpf"
            value={form.cpf}
            onChange={(e) => setForm((f) => ({ ...f, cpf: formatCpf(e.target.value) }))}
            placeholder="000.000.000-00"
            inputMode="numeric"
            aria-invalid={Boolean(errors.cpf)}
          />
          {errors.cpf && <span className="field-error">{errors.cpf}</span>}
        </div>

        <div className="field">
          <label htmlFor="email">E-mail</label>
          <input
            id="email"
            type="email"
            value={form.email}
            onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
            aria-invalid={Boolean(errors.email)}
          />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </div>

        <div className="field">
          <label htmlFor="birthDate">Data de nascimento</label>
          <input
            id="birthDate"
            type="date"
            value={form.birthDate}
            onChange={(e) => setForm((f) => ({ ...f, birthDate: e.target.value }))}
            aria-invalid={Boolean(errors.birthDate)}
          />
          {errors.birthDate && <span className="field-error">{errors.birthDate}</span>}
        </div>

        <button type="submit" className="btn btn-primary" disabled={submitting}>
          {submitting ? 'Confirmando...' : 'Confirmar reserva'}
        </button>
      </form>
    </div>
  )
}
