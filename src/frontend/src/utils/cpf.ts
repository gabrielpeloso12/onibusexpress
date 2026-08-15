/**
 * Validação de CPF espelhando o algoritmo usado no backend (Domain/ValueObjects/Cpf.cs),
 * para dar feedback imediato ao usuário sem depender de uma chamada à API.
 * A validação definitiva continua acontecendo no servidor.
 */

function extractDigits(value: string): string {
  return value.replace(/\D/g, '')
}

function calculateCheckDigit(numbers: number[], length: number): number {
  let sum = 0
  let weight = length + 1

  for (let i = 0; i < length; i++) {
    sum += numbers[i] * weight
    weight--
  }

  const remainder = sum % 11
  return remainder < 2 ? 0 : 11 - remainder
}

/** Valida formato e dígitos verificadores. Aceita dígitos "crus" ou formatados (000.000.000-00). */
export function isValidCpf(value: string): boolean {
  const digits = extractDigits(value)

  if (digits.length !== 11) return false
  if (new Set(digits).size === 1) return false // ex.: "00000000000"

  const numbers = digits.split('').map(Number)

  const firstCheckDigit = calculateCheckDigit(numbers, 9)
  if (firstCheckDigit !== numbers[9]) return false

  const secondCheckDigit = calculateCheckDigit(numbers, 10)
  return secondCheckDigit === numbers[10]
}

export function formatCpf(value: string): string {
  const digits = extractDigits(value).slice(0, 11)

  if (digits.length <= 3) return digits
  if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`
  if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`
  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`
}
