const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value)
}

const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

/** Formata uma data/hora ISO sem timezone (ex.: "2026-08-20T08:00:00") como horário local da viagem. */
export function formatDateTime(isoDateTime: string): string {
  return dateTimeFormatter.format(new Date(isoDateTime))
}

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
})

export function formatDate(isoDate: string): string {
  return dateFormatter.format(new Date(`${isoDate}T00:00:00`))
}

/** Converte "06:00:00" (TimeSpan do backend) em "6h" / "1h 30min". */
export function formatDuration(timeSpan: string): string {
  const [hoursPart, minutesPart] = timeSpan.split(':')
  const hours = Number(hoursPart)
  const minutes = Number(minutesPart)

  if (minutes === 0) return `${hours}h`
  return `${hours}h ${minutes}min`
}
