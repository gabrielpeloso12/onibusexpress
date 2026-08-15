import { describe, expect, it } from 'vitest'
import { formatCpf, isValidCpf } from '../../utils/cpf'

describe('isValidCpf', () => {
  it.each(['529.982.247-25', '52998224725', '111.444.777-35'])('aceita CPF válido: %s', (cpf) => {
    expect(isValidCpf(cpf)).toBe(true)
  })

  it.each([
    '529.982.247-24', // dígito verificador errado
    '00000000000', // dígitos repetidos
    '123456789', // curto demais
    '',
  ])('rejeita CPF inválido: %s', (cpf) => {
    expect(isValidCpf(cpf)).toBe(false)
  })
})

describe('formatCpf', () => {
  it('formata dígitos crus como 000.000.000-00 progressivamente', () => {
    expect(formatCpf('529')).toBe('529')
    expect(formatCpf('529982')).toBe('529.982')
    expect(formatCpf('529982247')).toBe('529.982.247')
    expect(formatCpf('52998224725')).toBe('529.982.247-25')
  })

  it('ignora caracteres não numéricos e limita a 11 dígitos', () => {
    expect(formatCpf('529.982.247-25extra')).toBe('529.982.247-25')
  })
})
