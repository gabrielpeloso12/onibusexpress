import styles from './Stepper.module.css'

interface StepperProps {
  steps: string[]
  /** Índice (0-based) do passo atual. */
  currentStep: number
}

export function Stepper({ steps, currentStep }: StepperProps) {
  return (
    <ol className={styles.stepper} aria-label="Progresso da compra">
      {steps.map((step, index) => {
        const state = index === currentStep ? 'current' : index < currentStep ? 'done' : 'pending'
        return (
          <li key={step} className={styles.step} data-state={state} aria-current={index === currentStep ? 'step' : undefined}>
            <span className={styles.bullet}>{index < currentStep ? '✓' : index + 1}</span>
            <span>{step}</span>
          </li>
        )
      })}
    </ol>
  )
}
